// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// One entry in the globally-ordered list of test iterations to execute.
    /// </summary>
    internal sealed class TestRunEntry
    {
        internal TestRunEntry(TestCase testCase, Type testClassType, object[] testData, int iterationIndex)
        {
            this.TestCase = testCase;
            this.TestClassType = testClassType;
            this.TestData = testData;
            this.IterationIndex = iterationIndex;
        }

        internal TestCase TestCase { get; }
        internal Type TestClassType { get; }
        internal object[] TestData { get; }
        internal int IterationIndex { get; }
    }

    /// <summary>
    /// Launches the bundle once and drives all test iterations through the burn unittest pipe protocol.
    /// </summary>
    internal static class BurnBATestRunner
    {
        private static readonly string _pipePassword = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Runs all test entries against the bundle.
        /// A single burn process is started; each iteration corresponds to one burn "session"
        /// (detect → plan → apply → shutdown cycle).  When the last iteration completes, the
        /// runner sends EngineMessageQuit to tell burn not to restart.
        /// </summary>
        internal static async Task RunAllAsync(
            IReadOnlyList<TestRunEntry> entries,
            string bundlePath,
            string password,
            IFrameworkHandle frameworkHandle,
            CancellationToken ct)
        {
            if (entries.Count == 0)
            {
                return;
            }

            // Unique pipe base name for this run.
            var pipeName = "BurnBATest-" + Guid.NewGuid().ToString("N");

            var bundleProcess = StartBundle(bundlePath, password, pipeName);
            try
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    bool isLast = i == entries.Count - 1;

                    var result = new TestResult(entry.TestCase)
                    {
                        StartTime = DateTimeOffset.UtcNow,
                    };

                    try
                    {
                        await RunOneIterationAsync(entry, pipeName, i == 0, isLast, bundleProcess, frameworkHandle, ct)
                            .ConfigureAwait(false);

                        result.Outcome = TestOutcome.Passed;
                    }
                    catch (OperationCanceledException)
                    {
                        result.Outcome = TestOutcome.None;
                        result.ErrorMessage = "Test run was cancelled.";
                    }
                    catch (Exception ex)
                    {
                        result.Outcome = TestOutcome.Failed;
                        result.ErrorMessage = ex.Message;
                        result.ErrorStackTrace = ex.StackTrace;
                    }
                    finally
                    {
                        result.EndTime = DateTimeOffset.UtcNow;
                        result.Duration = result.EndTime - result.StartTime;
                        frameworkHandle.RecordResult(result);
                    }

                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            finally
            {
                // Ensure the bundle process is cleaned up.
                if (bundleProcess != null && !bundleProcess.HasExited)
                {
                    bundleProcess.Kill();
                }
                bundleProcess?.Dispose();
            }
        }

        private static Process StartBundle(string bundlePath, string password, string pipeName)
        {
            // Burn unittest command line: <bundle.exe> -burn.unittest <password> <pipeName> <pipeSecret> <testHostPID>
            // The pipe secret is the same as the password (the burn unittest mode uses password as secret).
            var pid = Process.GetCurrentProcess().Id;
            var args = $"-burn.unittest \"{EscapeArg(password)}\" \"{pipeName}\" \"{_pipePassword}\" {pid}";

            var psi = new ProcessStartInfo(bundlePath, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            return Process.Start(psi);
        }

        private static async Task RunOneIterationAsync(
            TestRunEntry entry,
            string pipeName,
            bool firstIteration,
            bool isLastIteration,
            Process bundleProcess,
            IFrameworkHandle frameworkHandle,
            CancellationToken ct)
        {
            // Connect to burn's .BA and .BAEngine pipes.
            using var conn = await BurnPipeConnection.ConnectAsync(pipeName, _pipePassword, firstIteration, ct)
                .ConfigureAwait(false);

            // Create the test instance and set its iteration data.
            var instance = (BurnBATestBase)Activator.CreateInstance(entry.TestClassType)!;
            instance.TestData = entry.TestData;
            instance.TestIteration = entry.IterationIndex;

            // Start the engine relay: forward engine RPC calls from the real BA back to burn.
            RealBAPipeServer realBAServer = null;
            Task engineRelayTask = null;

            // Pump messages until disconnect.
            while (!ct.IsCancellationRequested)
            {
                var (msgType, payload) = conn.ReadBAMessage();

                if (msgType == BurnProtocolConstants.PipeMessageDisconnect)
                {
                    break;
                }

                // StartRealBA is a special message: burn wants us to launch the real BA.
                if (msgType == BurnProtocolConstants.BaMessageStartRealBA)
                {
                    (realBAServer, engineRelayTask) = await HandleStartRealBAAsync(payload, conn, ct).ConfigureAwait(false);

                    // Respond to burn with a simple [apiVersion] result.
                    var resp = new BurnBufferWriter();
                    resp.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    conn.WriteBAResponse(0 /* S_OK */, resp.ToArray());
                    continue;
                }

                // Dispatch the message to the test instance.
                bool isShutdown = msgType == (uint)BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONSHUTDOWN;
                var (hr, responseData) = BurnBAMessageDispatcher.Dispatch(instance, msgType, payload, realBAServer);

                if (isShutdown && isLastIteration)
                {
                    // Write the OnShutdown response first, then send EngineMessageQuit.
                    conn.WriteBAResponse(hr, responseData);
                    await SendQuitAsync(conn, bundleProcess, ct).ConfigureAwait(false);
                }
                else
                {
                    conn.WriteBAResponse(hr, responseData);
                }
            }

            // Wait for the engine relay to finish.
            if (engineRelayTask != null)
            {
                await engineRelayTask.ConfigureAwait(false);
            }

            // Stop the real BA server.
            realBAServer?.Dispose();
        }

        private static async Task<(RealBAPipeServer server, Task relayTask)> HandleStartRealBAAsync(
            byte[] payload,
            BurnPipeConnection conn,
            CancellationToken ct)
        {
            // Deserialize the StartRealBA args.
            var a = new BurnBufferReader(payload);
            // payload = [cbArgs][argsBytes][cbResults][defaultResultsBytes]
            a.ReadUInt32(); // cbArgs (skip)
            a.ReadUInt32(); // apiVersion
            var baPath = a.ReadString()!;
            var nCmdShow = a.ReadInt32();

            // Create new pipe names and secret for the real BA.
            var realBAPipeName = "BurnBATestRealBA-" + Guid.NewGuid().ToString("N");

            // Create server-side pipes for the real BA to connect to.
            var realBAServer = RealBAPipeServer.Create(realBAPipeName);

            // Launch the real BA.
            var realBAArgs = $"-burn.ba.apiver {BurnProtocolConstants.ApiVersion} -burn.ba.pipe \"{realBAPipeName}\" \"{_pipePassword}\"";
            var psi = new ProcessStartInfo(baPath, realBAArgs)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(psi);

            // Wait for the real BA to connect.
            await realBAServer.WaitForClientConnectAsync(_pipePassword, ct).ConfigureAwait(false);

            // Start the engine relay task: forward engine RPC calls from real BA to burn.
            var relayTask = Task.Run(() => RelayEngineMessages(realBAServer, conn, ct), ct);

            return (realBAServer, relayTask);
        }

        /// <summary>
        /// Relays engine messages from the real BA's .BAEngine pipe to burn's .BAEngine pipe.
        /// This runs in a background task for the duration of each iteration.
        /// </summary>
        private static void RelayEngineMessages(RealBAPipeServer realBA, BurnPipeConnection conn, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var (msgType, data) = realBA.ReadEngineMessage();
                if (msgType == BurnProtocolConstants.PipeMessageDisconnect)
                {
                    break;
                }

                conn.EnginePipeLock.Wait(ct);
                try
                {
                    conn.SendEngineMessage(msgType, data);
                    var (hr, responseData) = conn.ReadEngineResponse();
                    realBA.WriteEngineResponse(hr, responseData);
                }
                finally
                {
                    conn.EnginePipeLock.Release();
                }
            }
        }

        private static async Task SendQuitAsync(BurnPipeConnection conn, Process bundleProcess, CancellationToken ct)
        {
            await conn.EnginePipeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // BAENGINE_UNITTESTQUIT_ARGS: [uint32 apiVersion][uint32 exitCode]
                var w = new BurnBufferWriter();
                w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                w.WriteUInt32(0u); // exitCode = 0
                conn.SendEngineMessage(BurnProtocolConstants.EngineMessageQuit, w.ToArray());
            }
            finally
            {
                conn.EnginePipeLock.Release();
            }
        }

        private static string EscapeArg(string arg)
            => arg.Replace("\"", "\\\"");
    }
}

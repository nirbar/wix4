namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// One entry in the globally-ordered list of test iterations to execute.
    /// </summary>
    internal sealed class TestRunEntry
    {
        internal TestRunEntry(TestCase testCase, Type testClassType, object[] testData, int iterationIndex, bool stopTestsOnError = false)
        {
            this.TestCase = testCase;
            this.TestClassType = testClassType;
            this.TestData = testData;
            this.IterationIndex = iterationIndex;
            this.StopTestsOnError = stopTestsOnError;
            this.LogFiles = new List<string>();
            this.TestClassAttribute = testClassType.GetCustomAttribute<BurnBATestClassAttribute>(inherit: false);
        }

        internal TestCase TestCase { get; }
        internal Type TestClassType { get; }
        internal object[] TestData { get; }
        internal int IterationIndex { get; }
        internal bool StopTestsOnError { get; }
        internal List<string> LogFiles { get; }
        internal BurnBATestClassAttribute TestClassAttribute {  get; }
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
            var entriesWithLast = new List<TestRunEntry>(entries);
            var lastTestType = typeof(LastTest);
            entriesWithLast.Add(new TestRunEntry(BurnBATestFrameworkDiscoverer.MakeLastTestCase(), lastTestType, null, 0, false));
            var pipeName = "BurnBATest-" + Guid.NewGuid().ToString("N");

            var bundleProcess = StartBundle(bundlePath, password, pipeName);
            bool stopRemaining = false;
            try
            {
                for (int i = 0; i < entriesWithLast.Count; i++)
                {
                    var entry = entriesWithLast[i];
                    if (entry.TestClassType == lastTestType)
                    {
                        await RunOneIterationAsync(entry, pipeName, false, bundleProcess, frameworkHandle, new CancellationToken())
                            .ConfigureAwait(false);
                        continue;
                    }

                    var result = new TestResult(entry.TestCase)
                    {
                        StartTime = DateTimeOffset.UtcNow,
                    };

                    if (!string.IsNullOrEmpty(entry.TestClassAttribute?.Skip))
                    {
                        result.Outcome = TestOutcome.Skipped;
                        result.ErrorMessage = entry.TestClassAttribute.Skip;
                        result.EndTime = DateTimeOffset.UtcNow;
                        frameworkHandle.RecordResult(result);
                        continue;
                    }
                    if (ct.IsCancellationRequested)
                    {
                        result.Outcome = TestOutcome.Skipped;
                        result.ErrorMessage = "Canceled";
                        result.EndTime = DateTimeOffset.UtcNow;
                        frameworkHandle.RecordResult(result);
                        continue;
                    }
                    if (stopRemaining)
                    {
                        result.Outcome = TestOutcome.Skipped;
                        result.ErrorMessage = "Skipped because a previous test with StopTestsOnError=true failed.";
                        result.EndTime = DateTimeOffset.UtcNow;
                        frameworkHandle.RecordResult(result);
                        continue;
                    }

                    try
                    {
                        await RunOneIterationAsync(entry, pipeName, i == 0, bundleProcess, frameworkHandle, ct)
                            .ConfigureAwait(false);

                        result.Outcome = TestOutcome.Passed;
                    }
                    catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
                    {
                        // OCE originated from test-override code, not from the framework's
                        // cancellation token — treat it as a test failure.
                        result.Outcome = TestOutcome.Failed;
                        result.ErrorMessage = ex.Message;
                        result.ErrorStackTrace = ex.StackTrace;

                        if (entry.StopTestsOnError)
                        {
                            stopRemaining = true;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // OCE from the framework's own ct — the run was cancelled externally.
                        result.Outcome = TestOutcome.None;
                        result.ErrorMessage = "Test run was cancelled.";
                    }
                    catch (Exception ex)
                    {
                        result.Outcome = TestOutcome.Failed;
                        result.ErrorMessage = ex.Message;
                        result.ErrorStackTrace = ex.StackTrace;

                        if (entry.StopTestsOnError)
                        {
                            stopRemaining = true;
                        }
                    }
                    finally
                    {
                        result.EndTime = DateTimeOffset.UtcNow;
                        result.Duration = result.EndTime - result.StartTime;
                        frameworkHandle.RecordResult(result);

                        if (entry.LogFiles.Any() && ((entry.TestClassAttribute.AttachLogs == AttachLogs.Always) || ((entry.TestClassAttribute.AttachLogs == AttachLogs.OnFailure) && (result.Outcome == TestOutcome.Failed))))
                        {
                            var attachmentSet = new AttachmentSet(BurnBATestFrameworkExecutor.ExecutorUri, "Log Files");
                            result.Attachments.Add(attachmentSet);
                            foreach (var log in entry.LogFiles)
                            {
                                attachmentSet.Attachments.Add(UriDataAttachment.CreateFrom(log, Path.GetFileNameWithoutExtension(log)));
                            }
                            frameworkHandle.RecordAttachments(result.Attachments);
                        }
                    }
                }
            }
            finally
            {
                // Ensure the bundle process is cleaned up.
                if (bundleProcess != null && !bundleProcess.HasExited)
                {
                    Thread.Sleep(1000);
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

        /// <returns>
        /// Returns when the iteration completes (burn has shut down and the test instance has
        /// been disposed).  Throws if the test instance recorded a failure exception.
        /// </returns>
        private static async Task RunOneIterationAsync(
            TestRunEntry entry,
            string pipeName,
            bool firstIteration,
            Process bundleProcess,
            IFrameworkHandle frameworkHandle,
            CancellationToken ct)
        {
            // Connect to burn's .BA and .BAEngine pipes.
            using var conn = await BurnPipeConnection.ConnectAsync(pipeName, _pipePassword, firstIteration, ct)
                .ConfigureAwait(false);

            // Create the test instance and set its iteration data.
            using (var instance = (BurnBATestBase)Activator.CreateInstance(entry.TestClassType))
            {
                instance.TestData = entry.TestData;
                instance.TestIteration = entry.IterationIndex;

                // Start the engine relay: forward engine RPC calls from the real BA back to burn.
                RealBAPipeServer realBAServer = null;
                Task engineRelayTask = null;

                // Pump messages until disconnect.
                bool engineQuitSent = false;
                bool lastTestSent = false;
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var (msgType, payload) = conn.ReadBAMessage();

                        if (msgType == BurnProtocolConstants.PipeMessageDisconnect)
                        {
                            if (!instance.EndTestAutoPilot)
                            {
                                realBAServer?.SendBAMessage((uint)BurnProtocolConstants.PipeMessageDisconnect, null);
                            }
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
                        var (hr, responseData) = BurnBAMessageDispatcher.Dispatch(instance, msgType, payload, realBAServer, conn);

                        if ((instance is LastTest) && instance.EndTestAutoPilot && !lastTestSent)
                        {
                            lastTestSent = true;
                            await SendLastTestAsync(conn, new CancellationToken()).ConfigureAwait(false);
                        }

                        if (instance._pendingEngineQuit && !engineQuitSent)
                        {
                            // Autopilot reached OnDetectComplete or OnPlanComplete or OnApplyComplete: burn is now
                            // parked waiting.  Write the response, shut the real BA down cleanly, then send Engine.Quit().
                            instance._pendingEngineQuit = false;
                            conn.WriteBAResponse(hr, responseData);

                            if (realBAServer != null)
                            {
                                // Disconnect the engine pipe first so the real BA cannot forward its
                                // own Engine.Quit() call through our relay after receiving OnShutdown.
                                realBAServer.DisconnectEnginePipe();
                                engineRelayTask = null;

                                // Send ONUNITTESTSHUTDOWN so the real BA can close its UI gracefully.
                                // Engine pipe is already disconnected; the BA must not use it.
                                SendUnitTestShutdownToRealBA(realBAServer);

                                realBAServer.Dispose();
                                realBAServer = null;
                            }

                            await SendQuitAsync(conn, ct).ConfigureAwait(false);
                            engineQuitSent = true;
                            continue;
                        }
                        else
                        {
                            conn.WriteBAResponse(hr, responseData);
                        }
                    }
                    catch (Exception ex)
                    {
                        instance.AddException(ex);
                    }
                }

                // Wait for the engine relay to finish.
                if (engineRelayTask != null)
                {
                    await engineRelayTask.ConfigureAwait(false);
                }

                // Stop the real BA server.
                realBAServer?.Dispose();

                // Finalize the result. Throws on errors.
                entry.LogFiles.AddRange(instance.GetLogFiles());
                instance.FinalizeResult();
            }
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
                uint msgType;
                byte[] data;
                try
                {
                    (msgType, data) = realBA.ReadEngineMessage();
                }
                catch (ObjectDisposedException)
                {
                    // Engine pipe was disconnected (e.g. during autopilot shutdown). Exit relay cleanly.
                    break;
                }
                catch (IOException)
                {
                    // Pipe was broken by the remote side.
                    break;
                }

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

        /// <summary>
        /// Sends an <c>OnUnitTestShutdown</c> message to the real BA so it can close its UI cleanly
        /// rather than hanging on its next pipe-read after the test autopilot stops
        /// forwarding messages. The engine pipe has already been disconnected before this is called.
        /// </summary>
        /// <remarks>
        ///   payload = [cbArgs=4][uint32 apiVersion][cbResults=4][uint32 apiVersion]
        /// </remarks>
        private static void SendUnitTestShutdownToRealBA(RealBAPipeServer realBAServer)
        {
            try
            {
                var w = new BurnBufferWriter();
                w.WriteUInt32(4u);                               // cbArgs (4 bytes for apiVersion only)
                w.WriteUInt32(BurnProtocolConstants.ApiVersion); // args.apiVersion
                w.WriteUInt32(4u);                               // cbResults (4 bytes for apiVersion only)
                w.WriteUInt32(BurnProtocolConstants.ApiVersion); // results.apiVersion

                realBAServer.SendBAMessage(
                    (uint)BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNITTESTSHUTDOWN,
                    w.ToArray());

                // Wait for the real BA to acknowledge the shutdown.
                realBAServer.ReadBAResponse();

                // Now disconnect the BA pipe as well.
                realBAServer.SendBAMessage((uint)BurnProtocolConstants.PipeMessageDisconnect, null);
            }
            catch { } // Ignore an exception to dump the real BA. Usualy the real BA calls Environment.Exit() so an exception is normal here.
        }

        private static async Task SendQuitAsync(BurnPipeConnection conn, CancellationToken ct)
        {
            await conn.EnginePipeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // BAENGINE_UNITTESTQUIT_ARGS: [uint32 apiVersion][uint32 exitCode]
                var w = new BurnBufferWriter();
                w.WriteUInt32(8u);                            // cbArgs (8 bytes for apiVersion, exit code)
                w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                w.WriteUInt32(0u); // exitCode = 0
                w.WriteUInt32(4u); // cbResults
                w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                conn.SendEngineMessage(BurnProtocolConstants.EngineMessageQuit, w.ToArray());
            }
            finally
            {
                conn.EnginePipeLock.Release();
            }
        }

        private static async Task SendLastTestAsync(BurnPipeConnection conn, CancellationToken ct)
        {
            await conn.EnginePipeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // BAENGINE_UNITTESTLASTTEST_ARGS: [uint32 apiVersion]
                var w = new BurnBufferWriter();
                w.WriteUInt32(4u);                            // cbArgs
                w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                w.WriteUInt32(4u); // cbResults
                w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                conn.SendEngineMessage(BurnProtocolConstants.EngineMessageMarkLastTest, w.ToArray());
            }
            finally
            {
                conn.EnginePipeLock.Release();
            }
        }

        private static string EscapeArg(string arg)
        {
            return arg.Replace("\"", "\\\"");
        }
    }
}

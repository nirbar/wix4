// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using WixToolset.BootstrapperApplicationApi;

    /// <summary>
    /// Implements <see cref="IEngine"/> by forwarding calls through the burn .BAEngine named pipe.
    /// Set on <see cref="BurnBATestBase.Engine"/> before <c>OnCreate</c> is called so that test
    /// overrides can call engine methods (get/set variables, log, detect, plan, apply, etc.)
    /// without touching the COM <see cref="IBootstrapperEngine"/> interface.
    /// </summary>
    /// <remarks>
    /// All methods are synchronous and block until burn responds.
    /// The <see cref="BurnPipeConnection.EnginePipeLock"/> semaphore serializes access between
    /// this class and the engine relay task (<c>RelayEngineMessages</c>).
    /// </remarks>
    internal sealed class TestEngine : IEngine
    {
        // BOOTSTRAPPER_ENGINE_MESSAGE enum values (from BootstrapperEngineTypes.h).
        private const uint MsgGetPackageCount        = 1;
        private const uint MsgGetVariableNumeric      = 2;
        private const uint MsgGetVariableString       = 3;
        private const uint MsgGetVariableVersion      = 4;
        private const uint MsgFormatString            = 5;
        private const uint MsgEscapeString            = 6;
        private const uint MsgEvaluateCondition       = 7;
        private const uint MsgLog                    = 8;
        private const uint MsgSendEmbeddedError       = 9;
        private const uint MsgSendEmbeddedProgress    = 10;
        private const uint MsgSendEmbeddedCustomMsg   = 11;
        private const uint MsgSetUpdate               = 12;
        private const uint MsgSetLocalSource          = 13;
        private const uint MsgSetDownloadSource       = 14;
        private const uint MsgSetVariableNumeric      = 15;
        private const uint MsgSetVariableString       = 16;
        private const uint MsgSetVariableVersion      = 17;
        private const uint MsgCloseSplashScreen       = 18;
        private const uint MsgDetect                  = 19;
        private const uint MsgPlan                    = 20;
        private const uint MsgElevate                 = 21;
        private const uint MsgApply                   = 22;
        // MsgQuit = 23 = BurnProtocolConstants.EngineMessageQuit
        private const uint MsgLaunchApprovedExe       = 24;
        private const uint MsgSetUpdateSource         = 25;
        private const uint MsgCompareVersions         = 26;
        private const uint MsgGetRelatedBundleVariable = 27;

        // HRESULT constants (from NativeMethods.cs in BootstrapperApplicationApi).
        private const int S_OK               = 0;
        private const int E_CANCELLED        = unchecked((int)0x800704c7);
        private const int E_ALREADYINITIALIZED = unchecked((int)0x800704df);
        private const int E_NOTFOUND         = unchecked((int)0x80070490);

        // Plan uses API version 7 to include plannedScope (WIX_7_BOOTSTRAPPER_APPLICATION_API_VERSION).
        private const uint ApiVersion7 = 7u;

        private readonly BurnPipeConnection _conn;

        internal TestEngine(BurnPipeConnection conn) => _conn = conn;

        // -----------------------------------------------------------------------
        // Wire-format helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Combines the args and results buffers into the flat packet that burn expects:
        /// <c>[uint32 cbArgs][argsBytes][uint32 cbResults][resultsBytes]</c>.
        /// </summary>
        private static byte[] BuildPacket(BurnBufferWriter args, BurnBufferWriter results)
        {
            var argsBytes = args.ToArray();
            var resultsBytes = results.ToArray();
            using var ms = new MemoryStream(8 + argsBytes.Length + resultsBytes.Length);
            ms.Write(BitConverter.GetBytes((uint)argsBytes.Length), 0, 4);
            ms.Write(argsBytes, 0, argsBytes.Length);
            ms.Write(BitConverter.GetBytes((uint)resultsBytes.Length), 0, 4);
            ms.Write(resultsBytes, 0, resultsBytes.Length);
            return ms.ToArray();
        }

        /// <summary>
        /// Acquires <see cref="BurnPipeConnection.EnginePipeLock"/>, sends the engine message,
        /// reads the response, then releases the lock.
        /// </summary>
        private (int hr, byte[] data) SendEngineRpc(uint msgType, byte[] packet)
        {
            _conn.EnginePipeLock.Wait();
            try
            {
                _conn.SendEngineMessage(msgType, packet);
                return _conn.ReadEngineResponse();
            }
            finally
            {
                _conn.EnginePipeLock.Release();
            }
        }

        /// <summary>Throws <see cref="Win32Exception"/> when <paramref name="hr"/> is not S_OK.</summary>
        private static void ThrowIfFailed(int hr)
        {
            if (hr < 0)
            {
                throw new Win32Exception(hr);
            }
        }

        /// <summary>
        /// Writes a string to the args writer using burn's native format:
        /// <c>[uint32 charCount][charCount * 2 UTF-16LE bytes]</c>.
        /// <c>null</c> is serialized as an empty string (charCount = 0) so that the native
        /// <c>BuffReadString</c> can consume it correctly.
        /// </summary>
        private static void WriteStringArg(BurnBufferWriter w, string value)
            => w.WriteString(value ?? string.Empty);

        // -----------------------------------------------------------------------
        // IEngine implementation
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public int PackageCount
        {
            get
            {
                var args = new BurnBufferWriter();
                args.WriteUInt32(BurnProtocolConstants.ApiVersion);

                var results = new BurnBufferWriter();
                results.WriteUInt32(BurnProtocolConstants.ApiVersion);

                var (hr, data) = SendEngineRpc(MsgGetPackageCount, BuildPacket(args, results));
                ThrowIfFailed(hr);

                var r = new BurnBufferReader(data);
                r.ReadUInt32(); // struct size
                return (int)r.ReadUInt32(); // cPackages
            }
        }

        /// <inheritdoc/>
        public void Apply(IntPtr hwndParent)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt64((ulong)(long)hwndParent);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgApply, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void CloseSplashScreen()
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgCloseSplashScreen, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public int CompareVersions(string version1, string version2)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, version1);
            WriteStringArg(args, version2);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgCompareVersions, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return r.ReadInt32(); // nResult
        }

        /// <inheritdoc/>
        public bool ContainsVariable(string name)
        {
            // Ask the engine for the string variable value; E_NOTFOUND means it does not exist.
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchValue hint = 0

            var (hr, _) = SendEngineRpc(MsgGetVariableString, BuildPacket(args, results));
            return hr != E_NOTFOUND;
        }

        /// <inheritdoc/>
        public void Detect() => this.Detect(IntPtr.Zero);

        /// <inheritdoc/>
        public void Detect(IntPtr hwndParent)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt64((ulong)(long)hwndParent);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgDetect, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public bool Elevate(IntPtr hwndParent)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt64((ulong)(long)hwndParent);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgElevate, BuildPacket(args, results));
            if (hr == S_OK || hr == E_ALREADYINITIALIZED)
            {
                return true;
            }
            if (hr == E_CANCELLED)
            {
                return false;
            }
            ThrowIfFailed(hr);
            return false; // unreachable
        }

        /// <inheritdoc/>
        public string EscapeString(string input)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, input);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchOut hint = 0

            var (hr, data) = SendEngineRpc(MsgEscapeString, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            r.ReadUInt32(); // cchOut
            return r.ReadString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public bool EvaluateCondition(string condition)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, condition);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgEvaluateCondition, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return r.ReadBool(); // f
        }

        /// <inheritdoc/>
        public string FormatString(string format)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, format);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchOut hint = 0

            var (hr, data) = SendEngineRpc(MsgFormatString, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            r.ReadUInt32(); // cchOut
            return r.ReadString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public long GetVariableNumeric(string name)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgGetVariableNumeric, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return (long)r.ReadUInt64(); // llValue
        }

        /// <inheritdoc/>
        public SecureString GetVariableSecureString(string name)
        {
            var value = this.GetVariableString(name);
            if (value == null)
            {
                return null;
            }

            var ss = new SecureString();
            foreach (var c in value)
            {
                ss.AppendChar(c);
            }
            ss.MakeReadOnly();
            return ss;
        }

        /// <inheritdoc/>
        public string GetVariableString(string name)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchValue hint = 0

            var (hr, data) = SendEngineRpc(MsgGetVariableString, BuildPacket(args, results));
            ThrowIfFailed(hr);

            if (data.Length == 0)
            {
                return string.Empty;
            }

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            r.ReadUInt32(); // cchValue
            return r.ReadString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public string GetVariableVersion(string name)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchValue hint = 0

            var (hr, data) = SendEngineRpc(MsgGetVariableVersion, BuildPacket(args, results));
            ThrowIfFailed(hr);

            if (data.Length == 0)
            {
                return string.Empty;
            }

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            r.ReadUInt32(); // cchValue
            return r.ReadString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public string GetRelatedBundleVariable(string bundleCode, string name)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, bundleCode);
            WriteStringArg(args, name);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);
            results.WriteUInt32(0u); // cchValue hint = 0

            var (hr, data) = SendEngineRpc(MsgGetRelatedBundleVariable, BuildPacket(args, results));
            ThrowIfFailed(hr);

            if (data.Length == 0)
            {
                return string.Empty;
            }

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            r.ReadUInt32(); // cchValue
            return r.ReadString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public void LaunchApprovedExe(IntPtr hwndParent, string approvedExeForElevationId, string arguments)
            => this.LaunchApprovedExe(hwndParent, approvedExeForElevationId, arguments, 0);

        /// <inheritdoc/>
        public void LaunchApprovedExe(IntPtr hwndParent, string approvedExeForElevationId, string arguments, int waitForInputIdleTimeout)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt64((ulong)(long)hwndParent);
            WriteStringArg(args, approvedExeForElevationId);
            WriteStringArg(args, arguments);
            args.WriteUInt32((uint)waitForInputIdleTimeout);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgLaunchApprovedExe, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string message)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt32((uint)level);
            WriteStringArg(args, message);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgLog, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void Plan(LaunchAction action, BundleScope plannedScope)
        {
            // Plan uses API version 7 because BAENGINE_PLAN_ARGS.plannedScope was added in WiX 7.
            var args = new BurnBufferWriter();
            args.WriteUInt32(ApiVersion7);
            args.WriteUInt32((uint)action);
            args.WriteUInt32((uint)plannedScope);

            var results = new BurnBufferWriter();
            results.WriteUInt32(ApiVersion7);

            var (hr, _) = SendEngineRpc(MsgPlan, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void Quit(int exitCode)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt32((uint)exitCode);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(BurnProtocolConstants.EngineMessageQuit, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public int SendEmbeddedError(int errorCode, string message, int uiHint)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt32((uint)errorCode);
            WriteStringArg(args, message);
            args.WriteUInt32((uint)uiHint);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgSendEmbeddedError, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return r.ReadInt32(); // nResult
        }

        /// <inheritdoc/>
        public int SendEmbeddedProgress(int progressPercentage, int overallPercentage)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt32((uint)progressPercentage);
            args.WriteUInt32((uint)overallPercentage);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgSendEmbeddedProgress, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return r.ReadInt32(); // nResult
        }

        /// <inheritdoc/>
        public int SendEmbeddedCustomMessage(int code, string message)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            args.WriteUInt32((uint)code);
            WriteStringArg(args, message);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, data) = SendEngineRpc(MsgSendEmbeddedCustomMsg, BuildPacket(args, results));
            ThrowIfFailed(hr);

            var r = new BurnBufferReader(data);
            r.ReadUInt32(); // struct size
            return r.ReadInt32(); // nResult
        }

        /// <inheritdoc/>
        public void SetDownloadSource(string packageOrContainerId, string payloadId, string url, string user, string password, string authorizationHeader)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, packageOrContainerId);
            WriteStringArg(args, payloadId);
            WriteStringArg(args, url);
            WriteStringArg(args, user);
            WriteStringArg(args, password);
            WriteStringArg(args, authorizationHeader);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetDownloadSource, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetLocalSource(string packageOrContainerId, string payloadId, string path)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, packageOrContainerId);
            WriteStringArg(args, payloadId);
            WriteStringArg(args, path);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetLocalSource, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetUpdate(string localSource, string downloadSource, long size, UpdateHashType hashType, string hash, string updatePackageId)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, localSource);
            WriteStringArg(args, downloadSource);
            args.WriteUInt64((ulong)size);
            args.WriteUInt32((uint)hashType);
            WriteStringArg(args, hash);
            WriteStringArg(args, updatePackageId);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetUpdate, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetUpdateSource(string url, string authorizationHeader)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, url);
            WriteStringArg(args, authorizationHeader);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetUpdateSource, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetVariableNumeric(string name, long value)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);
            args.WriteUInt64((ulong)value);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetVariableNumeric, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetVariableString(string name, SecureString value, bool formatted)
        {
            // Decode SecureString to plain string; the pipe is a local named pipe so in-process
            // transmission is acceptable here.
            var plainValue = string.Empty;
            if (value != null && value.Length > 0)
            {
                var bstr = Marshal.SecureStringToBSTR(value);
                try
                {
                    plainValue = Marshal.PtrToStringBSTR(bstr);
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(bstr);
                }
            }

            this.SetVariableStringCore(name, plainValue, formatted);
        }

        /// <inheritdoc/>
        public void SetVariableString(string name, string value, bool formatted)
            => this.SetVariableStringCore(name, value ?? string.Empty, formatted);

        private void SetVariableStringCore(string name, string value, bool formatted)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);
            WriteStringArg(args, value);
            args.WriteBool(formatted);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetVariableString, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }

        /// <inheritdoc/>
        public void SetVariableVersion(string name, string value)
        {
            var args = new BurnBufferWriter();
            args.WriteUInt32(BurnProtocolConstants.ApiVersion);
            WriteStringArg(args, name);
            WriteStringArg(args, value ?? string.Empty);

            var results = new BurnBufferWriter();
            results.WriteUInt32(BurnProtocolConstants.ApiVersion);

            var (hr, _) = SendEngineRpc(MsgSetVariableVersion, BuildPacket(args, results));
            ThrowIfFailed(hr);
        }
    }
}

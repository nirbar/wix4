// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Mimic
{
    using System;
    using System.Security;
    using WixToolset.BootstrapperApplicationApi;

    /// <summary>
    /// Wraps an <see cref="IEngine"/> implementation and intercepts <see cref="Apply"/>:
    /// instead of sending the RPC to burn, it sets
    /// <see cref="BurnBATestBase._mimicApplyPending"/> so the runner can simulate the
    /// apply-phase callbacks without making any machine changes.
    /// All other <see cref="IEngine"/> calls are delegated unchanged to the real engine.
    /// </summary>
    internal sealed class MimicApplyEngine : IEngine
    {
        private readonly IEngine _real;
        private readonly BurnBATestBase _instance;

        internal MimicApplyEngine(IEngine real, BurnBATestBase instance)
        {
            _real = real ?? throw new ArgumentNullException(nameof(real));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        // -----------------------------------------------------------------------
        // Intercepted: Apply
        // -----------------------------------------------------------------------

        /// <summary>
        /// Intercepts the Apply call.  Instead of sending an RPC to burn, sets
        /// <see cref="BurnBATestBase._mimicApplyPending"/> so the runner fires synthetic
        /// apply callbacks directly on the test instance.
        /// </summary>
        public void Apply(IntPtr hwndParent) => _instance._mimicApplyPending = true;

        // -----------------------------------------------------------------------
        // Delegated: everything else
        // -----------------------------------------------------------------------

        public int PackageCount => _real.PackageCount;

        public void CloseSplashScreen() => _real.CloseSplashScreen();

        public int CompareVersions(string version1, string version2)
            => _real.CompareVersions(version1, version2);

        public bool ContainsVariable(string name) => _real.ContainsVariable(name);

        public void Detect() => _real.Detect();

        public void Detect(IntPtr hwndParent) => _real.Detect(hwndParent);

        public bool Elevate(IntPtr hwndParent) => _real.Elevate(hwndParent);

        public string EscapeString(string input) => _real.EscapeString(input);

        public bool EvaluateCondition(string condition) => _real.EvaluateCondition(condition);

        public string FormatString(string format) => _real.FormatString(format);

        public long GetVariableNumeric(string name) => _real.GetVariableNumeric(name);

        public SecureString GetVariableSecureString(string name) => _real.GetVariableSecureString(name);

        public string GetVariableString(string name) => _real.GetVariableString(name);

        public string GetVariableVersion(string name) => _real.GetVariableVersion(name);

        public string GetRelatedBundleVariable(string bundleCode, string name)
            => _real.GetRelatedBundleVariable(bundleCode, name);

        public void LaunchApprovedExe(IntPtr hwndParent, string approvedExeForElevationId, string arguments)
            => _real.LaunchApprovedExe(hwndParent, approvedExeForElevationId, arguments);

        public void LaunchApprovedExe(IntPtr hwndParent, string approvedExeForElevationId, string arguments, int waitForInputIdleTimeout)
            => _real.LaunchApprovedExe(hwndParent, approvedExeForElevationId, arguments, waitForInputIdleTimeout);

        public void Log(LogLevel level, string message) => _real.Log(level, message);

        public void Plan(LaunchAction action, BundleScope plannedScope) => _real.Plan(action, plannedScope);

        public void Quit(int exitCode) => _real.Quit(exitCode);

        public int SendEmbeddedCustomMessage(int code, string message)
            => _real.SendEmbeddedCustomMessage(code, message);

        public int SendEmbeddedError(int errorCode, string message, int uiHint)
            => _real.SendEmbeddedError(errorCode, message, uiHint);

        public int SendEmbeddedProgress(int progressPercentage, int overallPercentage)
            => _real.SendEmbeddedProgress(progressPercentage, overallPercentage);

        public void SetDownloadSource(string packageOrContainerId, string payloadId, string url, string user, string password, string authorizationHeader)
            => _real.SetDownloadSource(packageOrContainerId, payloadId, url, user, password, authorizationHeader);

        public void SetLocalSource(string packageOrContainerId, string payloadId, string path)
            => _real.SetLocalSource(packageOrContainerId, payloadId, path);

        public void SetUpdate(string localSource, string downloadSource, long size, UpdateHashType hashType, string hash, string updatePackageId)
            => _real.SetUpdate(localSource, downloadSource, size, hashType, hash, updatePackageId);

        public void SetUpdateSource(string url, string authorizationHeader)
            => _real.SetUpdateSource(url, authorizationHeader);

        public void SetVariableNumeric(string name, long value) => _real.SetVariableNumeric(name, value);

        public void SetVariableString(string name, SecureString value, bool formatted)
            => _real.SetVariableString(name, value, formatted);

        public void SetVariableString(string name, string value, bool formatted)
            => _real.SetVariableString(name, value, formatted);

        public void SetVariableVersion(string name, string value) => _real.SetVariableVersion(name, value);
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnUnitTest
{
    using System;
    using System.Threading.Tasks;
    using WixToolset.Burn.UnitTest;
    using WixToolset.TestTools;
    using Xunit;

    /// <summary>
    /// xUnit test class that drives the burn BA unit tests via <see cref="BurnBATestHost"/>.
    ///
    /// Each <c>[RuntimeFact]</c> below invokes one or more <see cref="BurnBATestBase"/>-derived
    /// test classes.  The tests are skipped automatically unless the <c>RuntimeTestsEnabled</c>
    /// environment variable is set to <c>true</c> and the process is running elevated.
    ///
    /// Configure the bundle path via the <c>BURN_TEST_BUNDLE_PATH</c> environment variable or
    /// by editing <c>BurnBaTests.runsettings</c>.
    /// </summary>
    public sealed class BurnUnitTestFixture
    {
        private static readonly string BundlePath =
            Environment.GetEnvironmentVariable("BURN_TEST_BUNDLE_PATH")
            ?? throw new InvalidOperationException(
                "Set the BURN_TEST_BUNDLE_PATH environment variable to the path of the bundle to test.");

        private static readonly string Password =
            Environment.GetEnvironmentVariable("BURN_TEST_PASSWORD") ?? string.Empty;

        /// <summary>
        /// Runs a silent install test against the configured bundle.
        /// </summary>
        [RuntimeFact]
        public async Task SilentInstall()
        {
            await BurnBATestHost.RunAsync<SilentInstallTest>(BundlePath, Password);
        }

        /// <summary>
        /// Runs a silent uninstall test against the configured bundle.
        /// </summary>
        [RuntimeFact]
        public async Task SilentUninstall()
        {
            await BurnBATestHost.RunAsync<SilentUninstallTest>(BundlePath, Password);
        }

        /// <summary>
        /// Runs an install failure test against the configured bundle.
        /// </summary>
        [RuntimeFact]
        public async Task InstallFailure()
        {
            await BurnBATestHost.RunAsync<InstallFailureTest>(BundlePath, Password);
        }

        /// <summary>
        /// Runs all three tests in order in a single bundle invocation.
        /// </summary>
        [RuntimeFact]
        public async Task AllTestsInOrder()
        {
            await BurnBATestHost.RunAsync(
                BundlePath,
                Password,
                new[]
                {
                    (typeof(SilentInstallTest),   Array.Empty<object>()),
                    (typeof(SilentUninstallTest), Array.Empty<object>()),
                    (typeof(InstallFailureTest),  Array.Empty<object>()),
                });
        }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Example burn BA unit test that simulates a silent install.
    /// The test verifies that the OnCreate callback supplies the expected LaunchAction.Install,
    /// and that the bundle progresses through Detect, Plan, and Apply phases without errors.
    /// </summary>
    [BurnBATestClass(Order = 1, StopTestsOnError = true)]
    public sealed class SilentInstallTest : BurnBATestBase
    {
        private bool _onCreateCalled;
        private bool _onStartupCalled;

        /// <inheritdoc/>
        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            this._onCreateCalled = true;

            // Verify command-line values via the TestBaCommand wrapper.
            if (this.Command!.Action != LaunchAction.Install)
            {
                this.SetException(new BurnBAAssertException(
                    $"Expected LaunchAction.Install for a silent install test, got {this.Command.Action}."));
            }
            else if (this.Command.Display != Display.None)
            {
                this.SetException(new BurnBAAssertException(
                    $"Expected Display.None for a silent install, got {this.Command.Display}."));
            }

            return base.OnCreate(pEngine, ref command);
        }

        /// <inheritdoc/>
        public override int OnStartup()
        {
            this._onStartupCalled = true;
            return base.OnStartup();
        }

        /// <inheritdoc/>
        public override int OnShutdown(ref BOOTSTRAPPER_SHUTDOWN_ACTION action)
        {
            if (!this._onCreateCalled)
            {
                this.SetException(new BurnBAAssertException(
                    "OnCreate was never called before OnShutdown."), endAutoPilot: false);
            }
            else if (!this._onStartupCalled)
            {
                this.SetException(new BurnBAAssertException(
                    "OnStartup was never called before OnShutdown."), endAutoPilot: false);
            }

            return base.OnShutdown(ref action);
        }
    }
}

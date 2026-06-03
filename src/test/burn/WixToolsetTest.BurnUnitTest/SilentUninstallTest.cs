// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Example burn BA unit test that simulates a silent uninstall.
    /// Verifies that burn sends LaunchAction.Uninstall and that the bundle shuts down cleanly.
    /// </summary>
    [BurnBATestClass(Order = 2, RequireAdmin = true)]
    public sealed class SilentUninstallTest : BurnBATestBase
    {
        private bool _detectCompleteCalled;

        /// <inheritdoc/>
        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            var uninstallCmd = new TestBaCommand(command);
            uninstallCmd.Action = LaunchAction.Uninstall;
            uninstallCmd.Display = Display.None;

            var cmd = uninstallCmd.ToCommand();
            return base.OnCreate(pEngine, ref cmd);
        }

        /// <inheritdoc/>
        public override int OnDetectComplete(int hrStatus, bool fEligibleForCleanup)
        {
            this._detectCompleteCalled = true;

            // S_OK == 0 means detection succeeded.
            if (hrStatus != 0)
            {
                this.SetException(new BurnBAAssertException(
                    $"DetectComplete reported failure: 0x{hrStatus:X8}"));
            }

            return base.OnDetectComplete(hrStatus, fEligibleForCleanup);
        }

        /// <inheritdoc/>
        public override int OnApplyComplete(
            int hrStatus,
            ApplyRestart restart,
            BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation,
            ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION action)
        {
            if (!this._detectCompleteCalled)
            {
                this.SetException(new BurnBAAssertException(
                    "OnDetectComplete should have been called before OnApplyComplete."),
                    endAutoPilot: false);
            }
            else if (hrStatus != 0)
            {
                this.SetException(new BurnBAAssertException(
                    $"ApplyComplete reported failure: 0x{hrStatus:X8}"),
                    endAutoPilot: false);
            }

            return base.OnApplyComplete(hrStatus, restart, recommendation, ref action);
        }
    }
}

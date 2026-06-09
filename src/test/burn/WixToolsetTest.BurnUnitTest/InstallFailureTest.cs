// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Example burn BA unit test that simulates a failed installation.
    /// When the apply phase fails the test verifies that OnApplyComplete receives a
    /// non-zero HRESULT and that the recommended action is reported correctly.
    /// </summary>
    [BurnBATestClass(Order = 3, RequireAdmin = true, Skip = "Not implemented yet")]
    public sealed class InstallFailureTest : BurnBATestBase
    {
        private int _applyCompleteHr;

        /// <inheritdoc/>
        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            var installCmd = new TestBaCommand(command);
            installCmd.Action = LaunchAction.Install;
            installCmd.Display = Display.None;

            var cmd = installCmd.ToCommand();
            return base.OnCreate(pEngine, ref cmd);
        }

        /// <inheritdoc/>
        public override int OnApplyComplete(
            int hrStatus,
            ApplyRestart restart,
            BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation,
            ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION action)
        {
            this._applyCompleteHr = hrStatus;

            // In a failure scenario we expect a non-zero HRESULT.
            if (hrStatus == 0)
            {
                // Record the failure but do NOT engage autopilot — we are already in
                // OnApplyComplete so burn will proceed to shutdown on its own.
                this.AddException(new BurnBAAssertException(
                    "Expected a failure HRESULT from the engine, but got S_OK."),
                    endAutoPilot: false);
            }

            // Allow burn to proceed normally (no custom override of the action).
            return base.OnApplyComplete(hrStatus, restart, recommendation, ref action);
        }

        /// <inheritdoc/>
        public override void FinalizeResult()
        {
            // Confirm the failure was seen.
            if (this._applyCompleteHr == 0)
            {
                this.AddException(new BurnBAAssertException(
                    "OnApplyComplete should have been called with a failure code before shutdown."),
                    endAutoPilot: false);
            }

            base.FinalizeResult();
        }
    }
}

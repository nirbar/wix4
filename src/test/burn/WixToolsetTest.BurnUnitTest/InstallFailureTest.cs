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
    [BurnBATestClass(Order = 3)]
    public sealed class InstallFailureTest : BurnBATestBase
    {
        private int _applyCompleteHr;

        /// <inheritdoc/>
        public override int OnApplyComplete(
            int hrStatus,
            ApplyRestart restart,
            BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation,
            ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION action)
        {
            this._applyCompleteHr = hrStatus;

            // In a failure scenario we expect a non-zero HRESULT.
            BurnAssert.NotEqual(0, hrStatus,
                "Expected a failure HRESULT from the engine, but got S_OK.");

            // Allow burn to proceed normally (no custom override of the action).
            return base.OnApplyComplete(hrStatus, restart, recommendation, ref action);
        }

        /// <inheritdoc/>
        public override int OnShutdown(ref BOOTSTRAPPER_SHUTDOWN_ACTION action)
        {
            // Confirm the failure was seen.
            BurnAssert.NotEqual(0, this._applyCompleteHr,
                "OnApplyComplete should have been called with a failure code before shutdown.");

            return base.OnShutdown(ref action);
        }
    }
}

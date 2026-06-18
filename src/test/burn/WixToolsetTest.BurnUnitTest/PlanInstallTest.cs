namespace WixToolsetTest.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Example burn BA unit test that simulates a silent install.
    /// The test verifies that the OnCreate callback supplies the expected LaunchAction.Install,
    /// and that the bundle progresses through Detect, Plan, and Apply phases without errors.
    /// </summary>
    [BurnBATestClass(Order = 0)]
    [BurnBAInlineData("a", "b")]
    [BurnBAInlineData("c", "d")]
    public sealed class PlanInstallTest : BurnBATestBase
    {
        private bool _onCreateCalled;
        private bool _onStartupCalled;
        private bool _onPlanCompleteCalled;

        /// <inheritdoc/>
        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            this._onCreateCalled = true;
            var installCmd = new TestBaCommand(command);
            installCmd.Action = LaunchAction.Install;
            installCmd.Display = Display.None;

            var cmd = installCmd.ToCommand();
            return base.OnCreate(pEngine, ref cmd);
        }

        /// <inheritdoc/>
        public override int OnStartup()
        {
            this._onStartupCalled = true;
            return base.OnStartup();
        }

        public override int OnPlanComplete(int hrStatus)
        {
            this.EndTestAutoPilot = true;
            this._onPlanCompleteCalled = true;
            return 0;
        }

        /// <inheritdoc/>
        public override void FinalizeResult()
        {
            if (!this._onCreateCalled)
            {
                this.AddException(new BurnBAAssertException(
                    "OnCreate was never called before OnShutdown."), endAutoPilot: false);
            }
            else if (!this._onStartupCalled)
            {
                this.AddException(new BurnBAAssertException(
                    "OnStartup was never called before OnShutdown."), endAutoPilot: false);
            }
            else if (!this._onPlanCompleteCalled)
            {
                this.AddException(new BurnBAAssertException(
                    "OnPlanComplete was never called before OnShutdown."), endAutoPilot: false);
            }
            base.FinalizeResult();
        }
    }
}

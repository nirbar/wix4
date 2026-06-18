namespace PanelSwWix4.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Example burn BA unit test that simulates a silent install plan.
    /// The test verifies that the bundle progresses through Detect and Plan phases without errors.
    /// </summary>
    [BurnBATestClass(Order = 0)]
    [BurnBAInlineData("MY_VARIABLE1", "a")]
    [BurnBAInlineData("MY_VARIABLE2", "b")]
    public sealed class PlanInstallTest : BurnBATestBase
    {
        private bool _onPlanCompleteCalled;

        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            var installCmd = new TestBaCommand(command);
            installCmd.Action = LaunchAction.Install;
            installCmd.Display = Display.None;
			installCmd.CommandLine = $"/silent /install {TestData[0].ToString()}={TestData[1].ToString()}";

            var cmd = installCmd.ToCommand();
            return base.OnCreate(pEngine, ref cmd);
        }

        public override int OnPlanComplete(int hrStatus)
        {
            this.EndTestAutoPilot = true;
            this._onPlanCompleteCalled = true;
            return 0;
        }

        public override void FinalizeResult()
        {
            base.FinalizeResult(); // Throw on first exception if occurred
            BurnAssert.True(this._onPlanCompleteCalled, "OnPlanComplete was not called.");
        }
    }
}
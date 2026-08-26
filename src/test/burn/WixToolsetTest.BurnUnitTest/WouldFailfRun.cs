namespace WixToolsetTest.BurnUnitTest
{
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// Unit test that always fails.
    /// It is filtered out in build `dotnet test` command line
    /// </summary>
    [BurnBATestClass()]
    public sealed class WouldFailfRun : BurnBATestBase
    {
        private bool _onStartupCalled;

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
        public override int OnStartup()
        {
            this._onStartupCalled = true;
            BurnAssert.Fail("This test fails all the time");
            return base.OnStartup();
        }

        /// <inheritdoc/>
        public override void FinalizeResult()
        {
            BurnAssert.True(this._onStartupCalled);
            base.FinalizeResult();
        }
    }
}

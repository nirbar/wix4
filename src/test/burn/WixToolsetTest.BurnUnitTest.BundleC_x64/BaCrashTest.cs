namespace WixToolsetTest.BurnUnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// The test verifies that the test framework can recover from RealBA process crash
    /// </summary>
    [BurnBATestClass(Order = Int32.MaxValue, AttachLogs = AttachLogs.Always)]
    public sealed class BaCrashTest : BurnBATestBase
    {
        /// <inheritdoc/>
        public override int OnCreate(IBootstrapperEngine pEngine, ref Command command)
        {
            var installCmd = new TestBaCommand(command);
            installCmd.Action = LaunchAction.Install;
            installCmd.Display = Display.None;
            installCmd.CommandLine = "/install /silent throwOnDetectComplete=1";

            var cmd = installCmd.ToCommand();
            return base.OnCreate(pEngine, ref cmd);
        }

        // If we got here then the BA hasn't crashed, which is a failure.
        public override int OnPlanBegin(int cPackages, ref bool fCancel)
        {
            this.AddException(new ShouldHaveCrashedException());
            return base.OnPlanBegin(cPackages, ref fCancel);
        }

        public override void FinalizeResult()
        {
            var exList = new List<Exception>(this.Exceptions.Where(e => e is ShouldHaveCrashedException));
            this.Exceptions = exList;
            base.FinalizeResult();
        }
    }

    public class ShouldHaveCrashedException : BurnBAAssertException
    {
        public ShouldHaveCrashedException()
            : base("Expected test to throw on BA process crash")
        { }
    }
}

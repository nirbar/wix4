namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    // Dummy test used to properly terminate all tests
    [BurnBATestClass(Order = Int32.MaxValue)]
    internal sealed class LastTest : BurnBATestBase
    {
        public override int OnCreate(IBootstrapperEngine engine, ref Command command)
        {
            this.EndTestAutoPilot = true;
            return base.OnCreate(engine, ref command);
        }

        public override void FinalizeResult()
        {
            // Never fail
        }
    }
}

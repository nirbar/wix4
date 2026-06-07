// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnUnitTest
{
    using System;
    using System.Linq;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest;

    /// <summary>
    /// The test verifies that the test framework can recover from RealBA process crash
    /// </summary>
    [BurnBATestClass(Order = Int32.MaxValue)]
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
            var ex = this.Exceptions.FirstOrDefault(e => e is ShouldHaveCrashedException);
            if (ex != null)
            {
                throw ex;
            }
        }
    }

    public class ShouldHaveCrashedException : System.Exception
    {
        public ShouldHaveCrashedException()
            : base("Expected test to throw on BA process crash")
        { }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

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

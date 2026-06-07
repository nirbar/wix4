// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.TestTools
{
    using System.IO;
    using WixToolset.TestSupport;

    public class TestExeTool : TestTool
    {
        private static readonly string TestExePath32 = TestData.Get("win-x86", "TestExe.exe");

        public TestExeTool()
            : base(TestExePath32)
        {
        }
    }
}

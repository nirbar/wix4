// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixTestTools
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using WixToolset.TestSupport;
    using Xunit;

    public class WixTestContext
    {
        static readonly string RootDataPath = Path.GetFullPath(TestData.Get("TestData"));

        public WixTestContext(ITestOutputHelper testOutputHelper)
        {
            this.TestOutputHelper = testOutputHelper;

            var testMethod = TestContext.Current.TestMethod;
            this.TestGroupName = testMethod?.TestClass?.TestClassSimpleName ?? "Unknown";
            this.TestName = testMethod?.MethodName ?? "Unknown";

            this.TestDataFolder = Path.Combine(RootDataPath, this.TestGroupName);
        }

        public string TestDataFolder { get; }

        /// <summary>
        /// Gets the name of the current test group.
        /// </summary>
        public string TestGroupName { get; }

        public string TestName { get; }

        public ITestOutputHelper TestOutputHelper { get; }

        /// <summary>
        /// Gets the test install directory for the current test.
        /// </summary>
        /// <param name="additionalPath">Additional subdirectories under the test install directory.</param>
        /// <returns>Full path to the test install directory.</returns>
        /// <remarks>
        /// The package or bundle must install into [ProgramFilesFolder]\~Test WiX\[TestGroupName]\([Additional]).
        /// </remarks>
        public string GetTestInstallFolder(bool x64, string additionalPath = null)
        {
            var baseDirectory = x64 ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.ProgramFilesX86;
            return Path.Combine(Environment.GetFolderPath(baseDirectory), "~Test WiX", this.TestGroupName, additionalPath ?? String.Empty);
        }

        /// <summary>
        /// Gets the test registry key for the current test.
        /// </summary>
        /// <param name="additionalPath">Additional subkeys under the test registry key.</param>
        /// <returns>Full path to the test registry key.</returns>
        /// <remarks>
        /// The package must write into HKLM\Software\WiX\Tests\[TestGroupName]\([Additional]).
        /// </remarks>
        public RegistryKey GetTestRegistryRoot(bool x64, string additionalPath = null)
        {
            var baseKey = x64 ? "Software" : @"Software\WOW6432Node";
            var key = String.Format(@"{0}\WiX\Tests\{1}\{2}", baseKey, this.TestGroupName, additionalPath ?? String.Empty);
            return Registry.LocalMachine.OpenSubKey(key, true);
        }

    }
}

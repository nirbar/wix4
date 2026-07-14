// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BuildTasks
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Build.Utilities;
    using WixToolset.TestSupport;
    using WixToolset.BuildTasks;
    using Xunit;

    public class GetMsiPropertyTaskFixture
    {
        [Fact]
        public void Execute_ReturnsPropertyValue_ForExistingProperty()
        {
            var folder = TestData.Get("TestData", "SimpleMsiPackage", "MsiPackage");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, "bin", "test.msi");
                var engine = new FakeBuildEngine();

                var buildTask = new WixBuild
                {
                    BuildEngine = engine,
                    SourceFiles = new[]
                    {
                        new TaskItem(Path.Combine(folder, "Package.wxs")),
                        new TaskItem(Path.Combine(folder, "PackageComponents.wxs")),
                    },
                    LocalizationFiles = new[]
                    {
                        new TaskItem(Path.Combine(folder, "Package.en-us.wxl")),
                    },
                    BindPaths = new[]
                    {
                        new TaskItem(Path.Combine(folder, "data")),
                    },
                    IntermediateDirectory = new TaskItem(intermediateFolder),
                    OutputFile = new TaskItem(msiPath),
                    DefaultCompressionLevel = "nOnE",
                    AcceptEula = "wix" + SomeVerInfo.Major,
                    ToolPath = WixBuildTaskFixture.PublishedWixExeFolder
                };

                Assert.True(buildTask.Execute(), $"WixBuild task failed unexpectedly. Output:\r\n{engine.Output}");
                Assert.True(File.Exists(msiPath));

                var getPropertyTask = new GetMsiProperty
                {
                    BuildEngine = engine,
                    MsiFile = new TaskItem(msiPath),
                    MsiProperty = "Manufacturer",
                };

                var result = getPropertyTask.Execute();

                Assert.True(result);
                WixAssert.StringEqual("Example Corporation", getPropertyTask.PropertyValue);
            }
        }

        [Fact]
        public void Execute_ReturnsFalse_ForMissingProperty()
        {
            var folder = TestData.Get("TestData", "SimpleMsiPackage", "MsiPackage");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, "bin", "test.msi");
                var engine = new FakeBuildEngine();

                var buildTask = new WixBuild
                {
                    BuildEngine = engine,
                    SourceFiles = new[]
                    {
                        new TaskItem(Path.Combine(folder, "Package.wxs")),
                        new TaskItem(Path.Combine(folder, "PackageComponents.wxs")),
                    },
                    LocalizationFiles = new[]
                    {
                        new TaskItem(Path.Combine(folder, "Package.en-us.wxl")),
                    },
                    BindPaths = new[]
                    {
                        new TaskItem(Path.Combine(folder, "data")),
                    },
                    IntermediateDirectory = new TaskItem(intermediateFolder),
                    OutputFile = new TaskItem(msiPath),
                    DefaultCompressionLevel = "nOnE",
                    AcceptEula = "wix" + SomeVerInfo.Major,
                    ToolPath = WixBuildTaskFixture.PublishedWixExeFolder
                };

                Assert.True(buildTask.Execute(), $"WixBuild task failed unexpectedly. Output:\r\n{engine.Output}");
                Assert.True(File.Exists(msiPath));

                var getPropertyEngine = new FakeBuildEngine();
                var getPropertyTask = new GetMsiProperty
                {
                    BuildEngine = getPropertyEngine,
                    MsiFile = new TaskItem(msiPath),
                    MsiProperty = "NONEXISTENT_PROPERTY_XYZ",
                };

                var result = getPropertyTask.Execute();

                Assert.False(result);
                Assert.Contains("MSI property 'NONEXISTENT_PROPERTY_XYZ' was not found in:", getPropertyEngine.Output);
            }
        }

        [Fact]
        public void Execute_ReturnsFalse_WhenMsiFileDoesNotExist()
        {
            var engine = new FakeBuildEngine();

            var task = new GetMsiProperty
            {
                BuildEngine = engine,
                MsiFile = new TaskItem(@"C:\nonexistent\path\test.msi"),
                MsiProperty = "Manufacturer",
            };

            var result = task.Execute();

            Assert.False(result);
            Assert.Contains("MSI file not found:", engine.Output);
        }
    }
}

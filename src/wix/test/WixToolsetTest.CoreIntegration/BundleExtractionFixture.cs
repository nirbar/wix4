// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System.IO;
    using System.Linq;
    using WixInternal.TestSupport;
    using WixInternal.Core.TestPackage;
    using WixToolset.Data;
    using Xunit;

    public class BundleExtractionFixture
    {
        [Fact]
        public void CanExtractBundleWithDetachedContainer()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                var extractFolderPath = Path.Combine(baseFolder, "extract");
                var baFolderPath = Path.Combine(extractFolderPath, "BA");
                var msiPath = Path.Combine(extractFolderPath, "_1", "test.msi");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleWithDetachedContainer", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "MinimalPackageGroup.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                });

                result.AssertSuccess();
                Assert.DoesNotContain(result.Messages, m => m.Level == MessageLevel.Warning);

                Assert.True(File.Exists(exePath));

                result = WixRunner.Execute(new[]
                {
                    "burn", "extract",
                    exePath,
                    "-oba", baFolderPath,
                    "-o", extractFolderPath
                });

                result.AssertSuccess();

                Assert.True(File.Exists(Path.Combine(baFolderPath, "manifest.xml")));
                Assert.True(File.Exists(msiPath));
            }
        }

        [Fact]
        public void CanExtractBundlePackageWithDetachedContainer()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var subexePath = Path.Combine(baseFolder, @"bin\subpackage.exe");
                var cabPath = Path.Combine(baseFolder, @"bin\_1");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleWithDetachedContainer", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "MinimalPackageGroup.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", subexePath,
                });

                result.AssertSuccess();
                Assert.DoesNotContain(result.Messages, m => m.Level == MessageLevel.Warning);

                Assert.True(File.Exists(subexePath));
                Assert.True(File.Exists(cabPath));

                using (var fsParent = new DisposableFileSystem())
                {
                    baseFolder = fsParent.GetFolder();
                    intermediateFolder = Path.Combine(baseFolder, "obj");
                    var exePath = Path.Combine(baseFolder, @"bin\test.exe");
                    var extractFolderPath = Path.Combine(baseFolder, "extract");
                    var baFolderPath = Path.Combine(extractFolderPath, "BA");
                    var subExePath = Path.Combine(extractFolderPath, "WixAttachedContainer", "subpackage.exe");
                    var subContainerPath = Path.Combine(extractFolderPath, "WixAttachedContainer", "_1");

                    result = WixRunner.Execute(new[]
                    {
                        "build",
                        Path.Combine(folder, "BundlePackageWithDetachedContainer", "Bundle.wxs"),
                        Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                        "-bindpath", Path.GetDirectoryName(subexePath),
                        "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                        "-bindpath", Path.Combine(folder, ".Data"),
                        "-intermediateFolder", intermediateFolder,
                        "-o", exePath,
                    });

                    result.AssertSuccess();
                    Assert.DoesNotContain(result.Messages, m => m.Level == MessageLevel.Warning);

                    Assert.True(File.Exists(exePath));
                    Assert.False(File.Exists(Path.Combine(baseFolder, @"bin", "subpackage.exe")));
                    Assert.True(File.Exists(Path.Combine(baseFolder, @"bin", "_1")));

                    result = WixRunner.Execute(new[]
                    {
                        "burn", "extract",
                        exePath,
                        "-oba", baFolderPath,
                        "-o", extractFolderPath
                    });

                    result.AssertSuccess();

                    Assert.True(File.Exists(Path.Combine(baFolderPath, "manifest.xml")));
                    Assert.True(File.Exists(subExePath));
                    Assert.False(File.Exists(subContainerPath));
                }
            }
        }
    }
}

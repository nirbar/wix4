// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System.IO;
    using System.Linq;
    using Example.Extension;
    using WixInternal.Core.TestPackage;
    using WixToolset.Data;
    using WixToolset.Data.Symbols;
    using WixToolset.TestSupport;
    using Xunit;

    /// <summary>
    /// Tests for the PreProcessFiles and PreProcessPayloads backend extension hooks.
    /// The example extension redirects every "sign_*" file to a copy with "SIGNED" appended.
    /// </summary>
    public class PreProcessHooksFixture
    {
        private const int SignedMarkerLength = 6;

        [Fact]
        public void PackageHookCanRedirectFilesToModifiedCopies()
        {
            var folder = TestData.Get("TestData", "PreProcessHooks");
            var dataFolder = Path.Combine(folder, "data");
            var originalPath = Path.Combine(dataFolder, "sign_example.txt");
            var originalLength = new FileInfo(originalPath).Length;

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Package.wxs"),
                    "-acceptEula", "wix" + SomeVerInfo.Major,
                    "-ext", ExtensionPaths.ExampleExtensionPath,
                    "-bindpath", dataFolder,
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, "bin", "test.msi")
                });

                result.AssertSuccess();

                var intermediate = Intermediate.Load(Path.Combine(baseFolder, "bin", "test.wixpdb"));
                var files = intermediate.Sections.Single().Symbols.OfType<FileSymbol>().ToDictionary(f => f.Id.Id);

                // The file was processed: sized from the copy and sourced from the intermediate folder.
                var signed = files["SignedFile"];
                Assert.Equal(originalLength + SignedMarkerLength, signed.FileSize);
                Assert.StartsWith(Path.Combine(intermediateFolder, "signed"), signed.Source.Path);
                Assert.Equal("sign_example.txt", signed.Name);

                // Other files are untouched.
                var plain = files["PlainFile"];
                Assert.Equal(new FileInfo(Path.Combine(dataFolder, "plain.txt")).Length, plain.FileSize);
                Assert.Equal(Path.Combine(dataFolder, "plain.txt"), plain.Source.Path);

                // The original is never modified.
                Assert.Equal(originalLength, new FileInfo(originalPath).Length);
            }
        }

        [Fact]
        public void BundleHookCanRedirectPayloadsToModifiedCopies()
        {
            var folder = TestData.Get("TestData", "PreProcessHooks");
            var dataFolder = Path.Combine(folder, "data");
            var originalPath = Path.Combine(dataFolder, "sign_payload.txt");
            var originalLength = new FileInfo(originalPath).Length;

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Bundle.wxs"),
                    "-ext", ExtensionPaths.ExampleExtensionPath,
                    "-bindpath", dataFolder,
                    "-bindpath", TestData.Get("TestData", ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-acceptEula", "wix" + SomeVerInfo.Major,
                    "-o", Path.Combine(baseFolder, "bin", "test.exe")
                });

                result.AssertSuccess();

                var intermediate = Intermediate.Load(Path.Combine(baseFolder, "bin", "test.wixpdb"));
                var payloads = intermediate.Sections.Single().Symbols.OfType<WixBundlePayloadSymbol>().ToDictionary(p => p.Id.Id);

                var signed = payloads["SignedPayload"];
                Assert.Equal(originalLength + SignedMarkerLength, signed.FileSize);
                Assert.StartsWith(Path.Combine(intermediateFolder, "signed"), signed.SourceFile.Path);
                Assert.Equal("sign_payload.txt", signed.Name);

                var plain = payloads["PlainPayload"];
                Assert.Equal(new FileInfo(Path.Combine(dataFolder, "plain.txt")).Length, plain.FileSize);
                Assert.Equal(Path.Combine(dataFolder, "plain.txt"), plain.SourceFile.Path);

                Assert.Equal(originalLength, new FileInfo(originalPath).Length);
            }
        }
    }
}

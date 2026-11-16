// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace WixToolsetTests.Dtf.CustomAction
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WixToolset.Dtf.CustomAction;

    [TestClass]
    public class ManagedCustomActionPackagerTests
    {
        private const string WindowsInstallerAssembly = "WixToolset.Dtf.WindowsInstaller.dll";

        private static string sampleAssembly;
        private static string toolsDirectory;
        private static string workRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            workRoot = Path.Combine(Path.GetTempPath(), "DtfCaTests_" + Path.GetRandomFileName());
            Directory.CreateDirectory(workRoot);

            toolsDirectory = StageTools(Path.Combine(workRoot, "tools"));
            sampleAssembly = CompileSampleCustomAction(Path.Combine(workRoot, "sample"));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            try { Directory.Delete(workRoot, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }

        [TestMethod]
        [DataRow(ManagedCustomActionPlatform.X86)]
        [DataRow(ManagedCustomActionPlatform.X64)]
        [DataRow(ManagedCustomActionPlatform.Arm64)]
        public void CanPackAndUnpackForEachPlatform(ManagedCustomActionPlatform platform)
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");

            ManagedCustomActionPackager.PackManagedCA(package, platform, InputFiles(), toolsDirectory);

            var unpacked = ManagedCustomActionPackager.UnpackManagedCA(package, Path.Combine(folder, "unpacked"));

            Assert.AreEqual(platform, unpacked.Platform);
            CollectionAssert.AreEqual(
                new[] { "SampleCA.dll", WindowsInstallerAssembly },
                unpacked.Files.Select(Path.GetFileName).ToArray());
            Assert.IsTrue(unpacked.Files.All(File.Exists));
        }

        [TestMethod]
        public void UnpackedFilesMatchTheOriginals()
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");
            var inputs = InputFiles();

            ManagedCustomActionPackager.PackManagedCA(package, ManagedCustomActionPlatform.X86, inputs, toolsDirectory);
            var unpacked = ManagedCustomActionPackager.UnpackManagedCA(package, Path.Combine(folder, "unpacked"));

            for (var i = 0; i < inputs.Count; ++i)
            {
                CollectionAssert.AreEqual(File.ReadAllBytes(inputs[i]), File.ReadAllBytes(unpacked.Files[i]), inputs[i]);
            }
        }

        [TestMethod]
        public void UnpackKeepsOrderOfAppearanceIncludingSupportFiles()
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");
            var config = Path.Combine(folder, "CustomAction.config");
            File.WriteAllText(config, "<configuration/>");

            var inputs = new List<string> { sampleAssembly, config, WindowsInstallerPath() };

            ManagedCustomActionPackager.PackManagedCA(package, ManagedCustomActionPlatform.X86, inputs, toolsDirectory);
            var unpacked = ManagedCustomActionPackager.UnpackManagedCA(package, Path.Combine(folder, "unpacked"));

            CollectionAssert.AreEqual(
                new[] { "SampleCA.dll", "CustomAction.config", WindowsInstallerAssembly },
                unpacked.Files.Select(Path.GetFileName).ToArray());
        }

        [TestMethod]
        public void CanRepackModifiedFilesUsingTheUnpackedList()
        {
            var folder = NewFolder();
            var original = Path.Combine(folder, "Original.CA.dll");
            var repacked = Path.Combine(folder, "Repacked.CA.dll");

            ManagedCustomActionPackager.PackManagedCA(original, ManagedCustomActionPlatform.X64, InputFiles(), toolsDirectory);
            var first = ManagedCustomActionPackager.UnpackManagedCA(original, Path.Combine(folder, "first"));

            // Stand in for signing: change the bytes of an inner file.
            File.AppendAllText(first.Files[1], "modified");
            var expected = File.ReadAllBytes(first.Files[1]);

            ManagedCustomActionPackager.PackManagedCA(repacked, first.Platform, first.Files, toolsDirectory);
            var second = ManagedCustomActionPackager.UnpackManagedCA(repacked, Path.Combine(folder, "second"));

            Assert.AreEqual(ManagedCustomActionPlatform.X64, second.Platform);
            CollectionAssert.AreEqual(
                first.Files.Select(Path.GetFileName).ToArray(),
                second.Files.Select(Path.GetFileName).ToArray());
            CollectionAssert.AreEqual(expected, File.ReadAllBytes(second.Files[1]));
        }

        [TestMethod]
        public void RepackedPackageKeepsTheCustomActionEntryPoints()
        {
            var folder = NewFolder();
            var original = Path.Combine(folder, "Original.CA.dll");
            var repacked = Path.Combine(folder, "Repacked.CA.dll");

            ManagedCustomActionPackager.PackManagedCA(original, ManagedCustomActionPlatform.X86, InputFiles(), toolsDirectory);
            var first = ManagedCustomActionPackager.UnpackManagedCA(original, Path.Combine(folder, "first"));
            ManagedCustomActionPackager.PackManagedCA(repacked, first.Platform, first.Files, toolsDirectory);

            // MakeSfxCA writes each entry point into the stub as "assembly!type.method".
            var entryPoint = Encoding.Unicode.GetBytes("SampleCA!SampleCustomActions.SampleAction");
            Assert.IsTrue(Contains(File.ReadAllBytes(original), entryPoint), "original");
            Assert.IsTrue(Contains(File.ReadAllBytes(repacked), entryPoint), "repacked");
        }

        [TestMethod]
        public void UnpackIgnoresDataAppendedAfterThePackage()
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");

            ManagedCustomActionPackager.PackManagedCA(package, ManagedCustomActionPlatform.X86, InputFiles(), toolsDirectory);

            // Signing appends a certificate table after the cabinet.
            using (var stream = new FileStream(package, FileMode.Append, FileAccess.Write))
            {
                stream.Write(new byte[4096], 0, 4096);
            }

            var unpacked = ManagedCustomActionPackager.UnpackManagedCA(package, Path.Combine(folder, "unpacked"));

            Assert.HasCount(2, unpacked.Files);
        }

        [TestMethod]
        public void UnpackRejectsFilesWithoutACabinet()
        {
            var folder = NewFolder();

            Assert.ThrowsExactly<InvalidDataException>(() =>
                ManagedCustomActionPackager.UnpackManagedCA(sampleAssembly, Path.Combine(folder, "unpacked")));
        }

        [TestMethod]
        public void UnpackRejectsFilesThatAreNotPortableExecutables()
        {
            var folder = NewFolder();
            var text = Path.Combine(folder, "text.dll");
            File.WriteAllText(text, "not a PE file");

            Assert.ThrowsExactly<InvalidDataException>(() =>
                ManagedCustomActionPackager.UnpackManagedCA(text, Path.Combine(folder, "unpacked")));
        }

        [TestMethod]
        public void PackFailsWhenTheWindowsInstallerAssemblyIsMissing()
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");

            var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
                ManagedCustomActionPackager.PackManagedCA(package, ManagedCustomActionPlatform.X86, new[] { sampleAssembly }, toolsDirectory));

            StringAssert.Contains(ex.Message, WindowsInstallerAssembly);
        }

        [TestMethod]
        public void PackFailsWhenTheToolsAreNotFound()
        {
            var folder = NewFolder();
            var package = Path.Combine(folder, "Test.CA.dll");

            Assert.ThrowsExactly<FileNotFoundException>(() =>
                ManagedCustomActionPackager.PackManagedCA(package, ManagedCustomActionPlatform.X86, InputFiles(), Path.Combine(folder, "missing")));
        }

        [TestMethod]
        public void PackRequiresAtLeastOneFile()
        {
            var folder = NewFolder();

            Assert.ThrowsExactly<ArgumentException>(() =>
                ManagedCustomActionPackager.PackManagedCA(Path.Combine(folder, "Test.CA.dll"), ManagedCustomActionPlatform.X86, new string[0], toolsDirectory));
        }

        private static IReadOnlyList<string> InputFiles() => new[] { sampleAssembly, WindowsInstallerPath() };

        private static string WindowsInstallerPath() => Path.Combine(AppContext.BaseDirectory, WindowsInstallerAssembly);

        private static string NewFolder()
        {
            var folder = Path.Combine(workRoot, Path.GetRandomFileName());
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Lays out MakeSfxCA and the SfxCA.dll stubs the way the NuGet package's tools folder does.
        /// The stubs are built by dtf.cmd into folders next to the managed output folder.
        /// </summary>
        private static string StageTools(string destination)
        {
            Directory.CreateDirectory(destination);

            var managedOutput = AppContext.BaseDirectory;
            foreach (var file in Directory.GetFiles(managedOutput, "WixToolset.Dtf.*"))
            {
                if (file.EndsWith(".exe") || file.EndsWith(".exe.config") || file.EndsWith(".dll"))
                {
                    File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
                }
            }

            var buildOutput = Path.GetFullPath(Path.Combine(managedOutput, ".."));
            foreach (var platform in new[] { "x86", "x64", "ARM64" })
            {
                var stub = Path.Combine(buildOutput, platform, "SfxCA.dll");
                Assert.IsTrue(File.Exists(stub), $"Missing {stub}. Build the native stubs first (dtf.cmd builds SfxCA\\sfxca_t.proj).");

                Directory.CreateDirectory(Path.Combine(destination, platform));
                File.Copy(stub, Path.Combine(destination, platform, "SfxCA.dll"), true);
            }

            Assert.IsTrue(File.Exists(Path.Combine(destination, "WixToolset.Dtf.MakeSfxCA.exe")), "Missing MakeSfxCA.exe in the test output.");
            return destination;
        }

        private static string CompileSampleCustomAction(string folder)
        {
            Directory.CreateDirectory(folder);
            var output = Path.Combine(folder, "SampleCA.dll");

            // Compiled beside its dependency, as MakeSfxCA expects.
            var windowsInstaller = Path.Combine(folder, WindowsInstallerAssembly);
            File.Copy(WindowsInstallerPath(), windowsInstaller, true);

            const string source = @"
using WixToolset.Dtf.WindowsInstaller;
public class SampleCustomActions
{
    [CustomAction]
    public static ActionResult SampleAction(Session session) { return ActionResult.Success; }
}";

            var parameters = new CompilerParameters(new[] { "System.dll", windowsInstaller }, output);
            using (var provider = new CSharpCodeProvider())
            {
                var results = provider.CompileAssemblyFromSource(parameters, source);
                Assert.IsFalse(results.Errors.HasErrors, string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select(e => e.ToString())));
            }

            return output;
        }

        private static bool Contains(byte[] source, byte[] find)
        {
            for (var i = 0; i <= source.Length - find.Length; ++i)
            {
                var j = 0;
                while (j < find.Length && source[i + j] == find[j])
                {
                    ++j;
                }

                if (j == find.Length)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

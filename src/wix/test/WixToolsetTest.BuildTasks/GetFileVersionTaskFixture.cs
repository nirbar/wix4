// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BuildTasks
{
    using System.IO;
    using Microsoft.Build.Utilities;
    using WixToolset.TestSupport;
    using WixToolset.BuildTasks;
    using Xunit;

    public class GetFileVersionTaskFixture
    {
        [Fact]
        public void Execute_ReturnsFileVersion_ForFileWithVersion()
        {
            // Use the WixToolset.BuildTasks assembly — it has a file version from the build.
            var assemblyPath = typeof(GetFileVersion).Assembly.Location;
            var engine = new FakeBuildEngine();

            var task = new GetFileVersion
            {
                BuildEngine = engine,
                FilePath = new TaskItem(assemblyPath),
            };

            var result = task.Execute();

            Assert.True(result);
            Assert.NotNull(task.FileVersion);
            Assert.NotEmpty(task.FileVersion);
        }

        [Fact]
        public void Execute_ReturnsFalse_WhenFileDoesNotExist()
        {
            var engine = new FakeBuildEngine();

            var task = new GetFileVersion
            {
                BuildEngine = engine,
                FilePath = new TaskItem(@"C:\nonexistent\path\missing.dll"),
            };

            var result = task.Execute();

            Assert.False(result);
            Assert.Contains("File not found:", engine.Output);
        }

        [Fact]
        public void Execute_ReturnsFalse_WhenFileHasNoVersion()
        {
            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var filePath = System.IO.Path.Combine(baseFolder, "noversion.txt");
                Directory.CreateDirectory(baseFolder);
                System.IO.File.WriteAllText(filePath, "no version info here");

                var engine = new FakeBuildEngine();

                var task = new GetFileVersion
                {
                    BuildEngine = engine,
                    FilePath = new TaskItem(filePath),
                };

                var result = task.Execute();

                Assert.False(result);
                Assert.Contains("No file version found in:", engine.Output);
            }
        }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.TestTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Wraps an MSBuild invocation for building WiX projects (.wixproj, .csproj, etc.)
    /// during installation unit tests.
    /// </summary>
    public class WixProjectBuilder
    {
        private readonly string projectPath;
        private readonly string outputPath;
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new <see cref="WixProjectBuilder"/> for the given project.
        /// </summary>
        /// <param name="projectPath">Full path to the project file (.wixproj, .csproj, etc.).</param>
        /// <param name="outputPath">Directory where build output (MSI, EXE, etc.) will be placed.</param>
        public WixProjectBuilder(string projectPath, string outputPath)
        {
            if (String.IsNullOrWhiteSpace(projectPath))
            {
                throw new ArgumentNullException(nameof(projectPath));
            }
            if (String.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentNullException(nameof(outputPath));
            }

            this.projectPath = projectPath;
            this.outputPath = outputPath;
            this.properties["Configuration"] = "Release";
            this.properties["Platform"] = "x64";
        }

        /// <summary>Sets the MSBuild Configuration property (default: Release).</summary>
        public WixProjectBuilder WithConfiguration(string configuration)
        {
            this.properties["Configuration"] = configuration;
            return this;
        }

        /// <summary>Sets the MSBuild Platform property (default: x64).</summary>
        public WixProjectBuilder WithPlatform(string platform)
        {
            this.properties["Platform"] = platform;
            return this;
        }

        /// <summary>
        /// Adds or overrides a WiX preprocessor variable passed as an MSBuild property
        /// (e.g. <c>DefineConstants</c> / <c>WixVariable</c>).
        /// </summary>
        public WixProjectBuilder WithPreprocessorVariable(string name, string value)
        {
            this.properties[$"WixVariable_{name}"] = value;
            return this;
        }

        /// <summary>
        /// Adds a binder search path (<c>BindPath</c>).
        /// </summary>
        public WixProjectBuilder WithBinderPath(string path)
        {
            if (this.properties.TryGetValue("BindPath", out var existing) && !String.IsNullOrEmpty(existing))
            {
                this.properties["BindPath"] = existing + ";" + path;
            }
            else
            {
                this.properties["BindPath"] = path;
            }
            return this;
        }

        /// <summary>Adds or overrides an arbitrary MSBuild property.</summary>
        public WixProjectBuilder WithProperty(string name, string value)
        {
            this.properties[name] = value;
            return this;
        }

        /// <summary>
        /// Runs MSBuild on the project and asserts a successful exit code.
        /// </summary>
        /// <param name="testOutputHelper">xunit output helper for build log lines.</param>
        /// <returns>
        /// Array of paths to the primary build output files (MSI, EXE, MSP, MSM)
        /// found under <see cref="outputPath"/> after the build.
        /// </returns>
        public string[] BuildAndGetOutput(ITestOutputHelper testOutputHelper)
        {
            if (testOutputHelper == null)
            {
                throw new ArgumentNullException(nameof(testOutputHelper));
            }

            Directory.CreateDirectory(this.outputPath);

            var msbuildArgs = BuildArguments();
            testOutputHelper.WriteLine($"WixProjectBuilder: dotnet msbuild {msbuildArgs}");

            var psi = new ProcessStartInfo("dotnet")
            {
                Arguments = $"msbuild {msbuildArgs}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    testOutputHelper.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    testOutputHelper.WriteLine($"STDERR: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Assert.True(0 == process.ExitCode, $"MSBuild failed with exit code {process.ExitCode} for project: {this.projectPath}");

            var outputExtensions = new[] { ".msi", ".exe", ".msp", ".msm" };
            return Directory.EnumerateFiles(this.outputPath)
                .Where(f => outputExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }

        private string BuildArguments()
        {
            var args = new List<string>
            {
                $"\"{this.projectPath}\"",
                $"/p:OutDir=\"{this.outputPath.TrimEnd('\\', '/')}\\\\\""
            };

            foreach (var kvp in this.properties)
            {
                args.Add($"/p:{kvp.Key}=\"{kvp.Value}\"");
            }

            return String.Join(" ", args);
        }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Dtf.CustomAction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using WixToolset.Dtf.Compression.Cab;

    /// <summary>
    /// Platform of a managed custom action package, which is the platform of its SfxCA.dll stub.
    /// </summary>
    public enum ManagedCustomActionPlatform
    {
        /// <summary>32-bit x86.</summary>
        X86,

        /// <summary>64-bit x64.</summary>
        X64,

        /// <summary>64-bit ARM.</summary>
        Arm64,
    }

    /// <summary>
    /// Result of <see cref="ManagedCustomActionPackager.UnpackManagedCA"/>.
    /// </summary>
    public sealed class UnpackedManagedCustomAction
    {
        internal UnpackedManagedCustomAction(ManagedCustomActionPlatform platform, IReadOnlyList<string> files)
        {
            this.Platform = platform;
            this.Files = files;
        }

        /// <summary>
        /// The platform of the unpacked package.
        /// </summary>
        public ManagedCustomActionPlatform Platform { get; }

        /// <summary>
        /// Full paths of the extracted files, in the order they appear in the package.
        /// The first file is the custom action assembly.
        /// </summary>
        public IReadOnlyList<string> Files { get; }
    }

    /// <summary>
    /// Unpacks and repacks managed custom action packages (<c>*.CA.dll</c>), for example, so
    /// the files inside can be signed before the package is rebuilt.
    /// </summary>
    /// <remarks>
    /// This assembly ships in the <c>tools</c> folder of the WixToolset.Dtf.CustomAction package,
    /// next to <c>WixToolset.Dtf.MakeSfxCA.exe</c> and the <c>SfxCA.dll</c> stubs. By default those
    /// are located relative to this assembly.
    /// </remarks>
    public static class ManagedCustomActionPackager
    {
        private const string MakeSfxCAFileName = "WixToolset.Dtf.MakeSfxCA.exe";
        private const int CabAlignment = 256; // The SfxCA stub only looks for the cabinet at multiples of this.
        private const int CabHeaderSize = 36;

        /// <summary>
        /// Unpacks a managed custom action package.
        /// </summary>
        /// <param name="caPath">Path to the <c>*.CA.dll</c> package.</param>
        /// <param name="outputDirectory">Directory to extract to. It is created if needed and must not contain files with the same names.</param>
        /// <returns>The files in order of appearance in the package, and the detected platform.</returns>
        public static UnpackedManagedCustomAction UnpackManagedCA(string caPath, string outputDirectory)
        {
            if (String.IsNullOrEmpty(caPath)) throw new ArgumentNullException(nameof(caPath));
            if (String.IsNullOrEmpty(outputDirectory)) throw new ArgumentNullException(nameof(outputDirectory));

            var bytes = File.ReadAllBytes(caPath);

            var platform = DetectPlatform(bytes, caPath);

            if (!TryFindCabinet(bytes, out var offset, out var length))
            {
                throw new InvalidDataException($"No appended cabinet found in '{caPath}'. It is not a managed custom action package.");
            }

            Directory.CreateDirectory(outputDirectory);

            // The cabinet engine reads cabinets from offset zero, so copy it out to a temporary file.
            var tempCab = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".cab");
            try
            {
                using (var stream = File.Create(tempCab))
                {
                    stream.Write(bytes, offset, length);
                }

                var cab = new CabInfo(tempCab);
                var files = new List<string>();

                foreach (var entry in cab.GetFiles())
                {
                    if (entry.Name.IndexOfAny(new[] { '\\', '/' }) >= 0)
                    {
                        throw new NotSupportedException($"The package contains '{entry.Name}' in a subfolder, which is not supported.");
                    }

                    files.Add(Path.Combine(outputDirectory, entry.Name));
                }

                cab.Unpack(outputDirectory);

                return new UnpackedManagedCustomAction(platform, files);
            }
            finally
            {
                try { File.Delete(tempCab); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        /// <summary>
        /// Packs files into a managed custom action package by running MakeSfxCA.
        /// </summary>
        /// <param name="outputPath">Path of the <c>*.CA.dll</c> to create.</param>
        /// <param name="platform">Platform, which selects the SfxCA.dll stub.</param>
        /// <param name="files">
        /// Files to pack. The first is the custom action assembly, and <c>WixToolset.Dtf.WindowsInstaller.dll</c>
        /// must be included. This is the list and order that <see cref="UnpackManagedCA"/> returns.
        /// </param>
        /// <param name="toolsDirectory">
        /// Optional folder containing <c>WixToolset.Dtf.MakeSfxCA.exe</c> and the <c>x86</c>, <c>x64</c> and <c>arm64</c>
        /// folders with <c>SfxCA.dll</c>. Defaults to the folder of this assembly.
        /// </param>
        public static void PackManagedCA(string outputPath, ManagedCustomActionPlatform platform, IReadOnlyList<string> files, string toolsDirectory = null)
        {
            if (String.IsNullOrEmpty(outputPath)) throw new ArgumentNullException(nameof(outputPath));
            if (files == null || files.Count == 0) throw new ArgumentException("At least the custom action assembly is required.", nameof(files));

            toolsDirectory = toolsDirectory ?? Path.GetDirectoryName(typeof(ManagedCustomActionPackager).Assembly.Location);

            var makeSfxCA = Path.Combine(toolsDirectory, MakeSfxCAFileName);
            var sfxCA = Path.Combine(toolsDirectory, StubFolder(platform), "SfxCA.dll");

            foreach (var required in new[] { makeSfxCA, sfxCA })
            {
                if (!File.Exists(required))
                {
                    throw new FileNotFoundException($"Cannot find '{Path.GetFileName(required)}' in '{Path.GetDirectoryName(required)}'.", required);
                }
            }

            outputPath = Path.GetFullPath(outputPath);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDirectory);

            // MakeSfxCA appends to the output file, so make sure it starts fresh.
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            // Use a response file, as the MSBuild targets do, to avoid command line length and quoting issues.
            var responseFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".rsp");
            try
            {
                var lines = new List<string>();
                foreach (var file in files)
                {
                    lines.Add("\"" + Path.GetFullPath(file) + "\"");
                }

                File.WriteAllLines(responseFile, lines, new UTF8Encoding(false));

                var startInfo = new ProcessStartInfo(makeSfxCA)
                {
                    Arguments = $"\"{outputPath}\" \"{sfxCA}\" \"@{responseFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var output = new StringBuilder();
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"MakeSfxCA failed with exit code {process.ExitCode}:{Environment.NewLine}{output}");
                    }
                }
            }
            finally
            {
                try { File.Delete(responseFile); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        private static string StubFolder(ManagedCustomActionPlatform platform)
        {
            switch (platform)
            {
                case ManagedCustomActionPlatform.X86: return "x86";
                case ManagedCustomActionPlatform.X64: return "x64";
                case ManagedCustomActionPlatform.Arm64: return "arm64";
                default: throw new ArgumentOutOfRangeException(nameof(platform));
            }
        }

        private static ManagedCustomActionPlatform DetectPlatform(byte[] pe, string path)
        {
            if (pe.Length < 0x40 || pe[0] != 'M' || pe[1] != 'Z')
            {
                throw new InvalidDataException($"'{path}' is not a PE file.");
            }

            var peOffset = BitConverter.ToInt32(pe, 0x3C);
            if (peOffset < 0 || peOffset + 6 > pe.Length || pe[peOffset] != 'P' || pe[peOffset + 1] != 'E')
            {
                throw new InvalidDataException($"'{path}' is not a PE file.");
            }

            switch (BitConverter.ToUInt16(pe, peOffset + 4))
            {
                case 0x014C: return ManagedCustomActionPlatform.X86;
                case 0x8664: return ManagedCustomActionPlatform.X64;
                case 0xAA64: return ManagedCustomActionPlatform.Arm64;
                default: throw new NotSupportedException($"'{path}' has an unsupported machine type.");
            }
        }

        /// <summary>
        /// Finds the cabinet the same way the SfxCA stub does, at offsets that are multiples of 256.
        /// A signature appended after signing makes the file longer than the cabinet, so the
        /// cabinet's own size is used instead of the rest of the file.
        /// </summary>
        private static bool TryFindCabinet(byte[] bytes, out int offset, out int length)
        {
            for (offset = CabAlignment; offset + CabHeaderSize <= bytes.Length; offset += CabAlignment)
            {
                if (bytes[offset] == 'M' && bytes[offset + 1] == 'S' && bytes[offset + 2] == 'C' && bytes[offset + 3] == 'F')
                {
                    var size = BitConverter.ToUInt32(bytes, offset + 8);
                    if (size >= CabHeaderSize && size <= (uint)(bytes.Length - offset))
                    {
                        length = (int)size;
                        return true;
                    }
                }
            }

            length = 0;
            return false;
        }
    }
}

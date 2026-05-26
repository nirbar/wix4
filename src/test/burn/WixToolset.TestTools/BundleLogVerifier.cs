// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.TestTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    /// <summary>
    /// Verifies bundle log files for expected burn messages.
    /// Searches across multiple log files when the bundle restarts (e.g. elevation) and
    /// creates new log files per-phase.
    /// </summary>
    public class BundleLogVerifier
    {
        private readonly IReadOnlyList<string> logPaths;

        public BundleLogVerifier(BundleInstaller installer)
        {
            this.logPaths = installer.LogFiles;
        }

        public BundleLogVerifier(IReadOnlyList<string> logPaths)
        {
            this.logPaths = logPaths ?? throw new ArgumentNullException(nameof(logPaths));
        }

        public BundleLogVerifier(string singleLogPath)
            : this(new[] { singleLogPath })
        {
        }

        /// <summary>
        /// Asserts that at least one log file contains a line matching the given regex pattern.
        /// </summary>
        public void AssertContainsPattern(string regexPattern, params string[] packageIds)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var logPath = GetPackageLogFile(packageIds);
            Assert.False(string.IsNullOrEmpty(logPath), "Could not find log file");
            Assert.True(File.Exists(logPath), $"File does not exist: {logPath}");

            foreach (var line in File.ReadLines(logPath))
            {
                if (regex.IsMatch(line))
                {
                    return;
                }
            }

            Assert.Fail($"Log file '{logPath}' does not contain the pattern: {regexPattern}");
        }

        /// <summary>
        /// Asserts that no log file contains a line matching the given regex pattern.
        /// </summary>
        public void AssertDoesNotContainPattern(string regexPattern, params string[] packageIds)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var logPath = GetPackageLogFile(packageIds);
            Assert.False(string.IsNullOrEmpty(logPath), "Could not find log file");
            Assert.True(File.Exists(logPath), $"File does not exist: {logPath}");

            foreach (var line in File.ReadLines(logPath))
            {
                if (regex.IsMatch(line))
                {
                    Assert.Fail($"Log file '{logPath}' contained unexpected line matching pattern '{regexPattern}':{Environment.NewLine}{line}");
                }
            }
        }

        /// <summary>
        /// Asserts that at least one log file contains the given literal text (case-insensitive).
        /// </summary>
        public void AssertContains(string text, params string[] packageIds)
        {
            var logPath = GetPackageLogFile(packageIds);
            Assert.False(string.IsNullOrEmpty(logPath), "Could not find log file");
            Assert.True(File.Exists(logPath), $"File does not exist: {logPath}");

            foreach (var line in File.ReadLines(logPath))
            {
                if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            Assert.Fail($"Log file '{logPath}' does not contain the text: {text}");
        }

        /// <summary>
        /// Attempt to find a log file corresponding to the package hierarchy.
        /// </summary>
        /// <param name="packageIds">If not specified, returns the main log file.
        /// If specified, attempt to find hierarhial package log. That is, packageIds[0] corresponds to a package's ID of the main bundle. 
        /// packageIds[1] corresponds to a subpackage of packageId[0], etc.
        /// </param>
        /// <returns>Path to the log file.</returns>
        public string GetPackageLogFile(params string[] packageIds)
        {
            string defaultLogFile = this.logPaths.FirstOrDefault();
            if ((packageIds == null) || (packageIds.Length == 0))
            {
                return defaultLogFile;
            }

            string logPattern = Path.GetFileNameWithoutExtension(defaultLogFile);
            foreach (var p in packageIds)
            {
                logPattern += $"_[0-9]{{3}}_{p}";
            }
            logPattern += @".*\.log$";
            var rgx = new Regex(logPattern);
            foreach (var l in this.logPaths)
            {
                if (rgx.IsMatch(l))
                {
                    return l;
                }
            }

            return defaultLogFile;
        }
    }
}

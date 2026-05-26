// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixTestTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        public void AssertContainsPattern(string regexPattern)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            foreach (var logPath in this.logPaths)
            {
                if (!File.Exists(logPath))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(logPath))
                {
                    if (regex.IsMatch(line))
                    {
                        return;
                    }
                }
            }

            Assert.Fail($"No log file contained a line matching pattern: {regexPattern}{Environment.NewLine}Searched logs:{Environment.NewLine}{String.Join(Environment.NewLine, this.logPaths)}");
        }

        /// <summary>
        /// Asserts that no log file contains a line matching the given regex pattern.
        /// </summary>
        public void AssertDoesNotContainPattern(string regexPattern)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            foreach (var logPath in this.logPaths)
            {
                if (!File.Exists(logPath))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(logPath))
                {
                    if (regex.IsMatch(line))
                    {
                        Assert.Fail($"Log file '{logPath}' contained unexpected line matching pattern '{regexPattern}':{Environment.NewLine}{line}");
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that at least one log file contains the given literal text (case-insensitive).
        /// </summary>
        public void AssertContains(string text)
        {
            foreach (var logPath in this.logPaths)
            {
                if (!File.Exists(logPath))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(logPath))
                {
                    if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }

            Assert.Fail($"No log file contained the text: {text}{Environment.NewLine}Searched logs:{Environment.NewLine}{String.Join(Environment.NewLine, this.logPaths)}");
        }
    }
}

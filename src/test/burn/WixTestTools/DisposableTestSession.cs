// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixTestTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using WixToolset.TestSupport;

    /// <summary>
    /// Tracks build artifacts, install paths and log files for a test session, and disposes
    /// (uninstalls / cleans up) everything on scope exit.
    /// </summary>
    public sealed class DisposableTestSession : IDisposable
    {
        private readonly List<string> installPaths = new List<string>();
        private readonly List<string> logPaths = new List<string>();
        private bool disposed;

        /// <summary>
        /// Gets the xunit test context associated with this session.
        /// </summary>
        public WixTestContext TestContext { get; }

        public DisposableTestSession(WixTestContext testContext)
        {
            this.TestContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
        }

        /// <summary>
        /// Registers an installed path so it will be cleaned up on disposal.
        /// </summary>
        public void TrackInstallPath(string path)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                this.installPaths.Add(path);
            }
        }

        /// <summary>
        /// Registers a log file path so it can be examined and then removed on disposal.
        /// </summary>
        public void TrackLogPath(string path)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                this.logPaths.Add(path);
            }
        }

        /// <summary>
        /// Returns all tracked log file paths.
        /// </summary>
        public IReadOnlyList<string> LogPaths => this.logPaths;

        /// <summary>
        /// Returns all tracked install paths.
        /// </summary>
        public IReadOnlyList<string> InstallPaths => this.installPaths;

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            // Remove tracked install directories.
            foreach (var path in this.installPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    this.TestContext.TestOutputHelper.WriteLine($"Warning: could not clean install path '{path}': {ex.Message}");
                }
            }

            // Remove tracked log files.
            foreach (var logPath in this.logPaths)
            {
                try
                {
                    if (File.Exists(logPath))
                    {
                        File.Delete(logPath);
                    }
                }
                catch (Exception ex)
                {
                    this.TestContext.TestOutputHelper.WriteLine($"Warning: could not delete log '{logPath}': {ex.Message}");
                }
            }
        }
    }
}

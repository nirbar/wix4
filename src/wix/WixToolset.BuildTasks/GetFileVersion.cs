// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.BuildTasks
{
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// MSBuild task that reads the file version from a binary file.
    /// </summary>
    public class GetFileVersion : Task
    {
        /// <summary>
        /// Path to the file whose version is to be read.
        /// </summary>
        [Required]
        public ITaskItem FilePath { get; set; }

        /// <summary>
        /// The file version string read from the file's version resource.
        /// </summary>
        [Output]
        public string FileVersion { get; private set; }

        /// <summary>
        /// Reads the file version from the specified file.
        /// </summary>
        /// <returns>True upon successful completion of the task.</returns>
        public override bool Execute()
        {
            var path = this.FilePath.ItemSpec;

            if (!File.Exists(path))
            {
                this.Log.LogError("File not found: {0}", path);
                return false;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(path);
            this.FileVersion = versionInfo.FileVersion;

            if (string.IsNullOrEmpty(this.FileVersion))
            {
                this.Log.LogError("No file version found in: {0}", path);
                return false;
            }

            return true;
        }
    }
}

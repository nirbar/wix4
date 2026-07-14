// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.BuildTasks
{
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using WixToolset.Dtf.WindowsInstaller;

    /// <summary>
    /// MSBuild task that reads a property value from an MSI database.
    /// </summary>
    public class GetMsiProperty : Task
    {
        /// <summary>
        /// Path to the MSI file.
        /// </summary>
        [Required]
        public ITaskItem MsiFile { get; set; }

        /// <summary>
        /// Name of the MSI property to read.
        /// </summary>
        [Required]
        public string MsiProperty { get; set; }

        /// <summary>
        /// The value of the requested MSI property.
        /// </summary>
        [Output]
        public string PropertyValue { get; private set; }

        /// <summary>
        /// Reads a property value from the MSI database.
        /// </summary>
        /// <returns>True upon successful completion of the task.</returns>
        public override bool Execute()
        {
            var msiPath = this.MsiFile.ItemSpec;

            if (!File.Exists(msiPath))
            {
                this.Log.LogError("MSI file not found: {0}", msiPath);
                return false;
            }

            try
            {
                using (var database = new Database(msiPath, DatabaseOpenMode.ReadOnly))
                {
                    this.PropertyValue = database.ExecuteScalar(
                        "SELECT `Value` FROM `Property` WHERE `Property` = '{0}'",
                        this.MsiProperty) as string;
                }
            }
            catch (InstallerException)
            {
                this.Log.LogError("MSI property '{0}' was not found in: {1}", this.MsiProperty, msiPath);
                return false;
            }

            return true;
        }
    }
}

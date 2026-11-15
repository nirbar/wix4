// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility.Data
{
    /// <summary>
    /// The kind of source file that will be placed in an output.
    /// </summary>
    public enum SourceFileKind
    {
        /// <summary>
        /// A file installed by a package (File table), including files from merge modules.
        /// </summary>
        File,

        /// <summary>
        /// A stream stored in the Binary table of a package, such as a custom action DLL.
        /// </summary>
        Binary,

        /// <summary>
        /// A stream stored in the Icon table of a package.
        /// </summary>
        Icon,

        /// <summary>
        /// A file stored in the MsiEmbeddedUI table of a package.
        /// </summary>
        EmbeddedUI,

        /// <summary>
        /// A payload of a bundle, including bootstrapper application payloads.
        /// </summary>
        Payload,
    }
}

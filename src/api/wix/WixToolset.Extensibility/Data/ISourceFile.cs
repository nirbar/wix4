// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Extensibility.Data
{
    using WixToolset.Data;

    /// <summary>
    /// A file on disk that will be placed in an output (package or bundle).
    /// </summary>
    public interface ISourceFile
    {
        /// <summary>
        /// The kind of file.
        /// </summary>
        SourceFileKind Kind { get; }

        /// <summary>
        /// Identifier of the file, unique within its <see cref="Kind"/>.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Source line number that defines the file.
        /// </summary>
        SourceLineNumber SourceLineNumber { get; }

        /// <summary>
        /// Path to the file. May be set to redirect the output to a different file, for example a signed copy.
        /// Changing the path does not change the identity or the name of the file in the output.
        /// </summary>
        string SourcePath { get; set; }
    }
}

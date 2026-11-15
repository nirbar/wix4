// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.WindowsInstaller.ExtensibilityServices
{
    using System;
    using WixToolset.Data;
    using WixToolset.Extensibility.Data;

    /// <summary>
    /// A <see cref="ISourceFile"/> backed by a path field of a symbol.
    /// </summary>
    internal class SymbolSourceFile : ISourceFile
    {
        private readonly Func<string> getPath;
        private readonly Action<string> setPath;

        public SymbolSourceFile(SourceFileKind kind, IntermediateSymbol symbol, Func<string> getPath, Action<string> setPath)
        {
            this.Kind = kind;
            this.Id = symbol.Id?.Id;
            this.SourceLineNumber = symbol.SourceLineNumbers;
            this.getPath = getPath;
            this.setPath = setPath;
        }

        public SourceFileKind Kind { get; }

        public string Id { get; }

        public SourceLineNumber SourceLineNumber { get; }

        public string SourcePath
        {
            get => this.getPath();
            set => this.setPath(value);
        }
    }
}

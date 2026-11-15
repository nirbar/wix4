// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.Burn
{
    using WixToolset.Data;
    using WixToolset.Data.Symbols;
    using WixToolset.Extensibility.Data;

    /// <summary>
    /// A <see cref="ISourceFile"/> backed by a bundle payload.
    /// </summary>
    internal class PayloadSourceFile : ISourceFile
    {
        private readonly WixBundlePayloadSymbol payload;

        public PayloadSourceFile(WixBundlePayloadSymbol payload)
        {
            this.payload = payload;
        }

        public SourceFileKind Kind => SourceFileKind.Payload;

        public string Id => this.payload.Id.Id;

        public SourceLineNumber SourceLineNumber => this.payload.SourceLineNumbers;

        public string SourcePath
        {
            get => this.payload.SourceFile?.Path;
            set => this.payload.SourceFile = new IntermediateFieldPathValue { Path = value, BaseUri = this.payload.SourceFile?.BaseUri };
        }
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using WixToolset.Data;
    using WixToolset.Data.Symbols;
    using WixToolset.Extensibility;

    /// <summary>
    /// Demonstrates <see cref="IBurnBackendBinderExtension.PreProcessPayloads"/>: payloads whose file
    /// name starts with "sign_" are redirected to a modified copy, leaving the original untouched.
    /// </summary>
    internal class ExampleBurnBackendExtension : BaseBurnBackendBinderExtension
    {
        public override void PreProcessPayloads(IntermediateSection section, IReadOnlyCollection<WixBundlePayloadSymbol> payloads)
        {
            foreach (var payload in payloads)
            {
                var source = payload.SourceFile?.Path;

                if (String.IsNullOrEmpty(source) || !Path.GetFileName(source).StartsWith("sign_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var folder = Path.Combine(this.Context.IntermediateFolder, "signed", payload.Id.Id);
                Directory.CreateDirectory(folder);

                var copy = Path.Combine(folder, Path.GetFileName(source));
                File.Copy(source, copy, overwrite: true);
                File.AppendAllText(copy, ExampleConstants.SignedMarker);

                payload.SourceFile = new IntermediateFieldPathValue { Path = copy };
            }
        }
    }
}

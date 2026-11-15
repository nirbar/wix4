// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System.Collections.Generic;
    using WixToolset.Data;
    using WixToolset.Extensibility;
    using WixToolset.Extensibility.Data;

    internal class ExampleBurnBackendExtension : BaseBurnBackendBinderExtension
    {
        public override void PreProcessSourceFiles(IntermediateSection section, IReadOnlyCollection<ISourceFile> files)
        {
            ExampleSigner.Sign(this.Context.IntermediateFolder, files);
        }
    }
}

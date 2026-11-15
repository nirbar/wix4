// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using WixToolset.Data;
    using WixToolset.Data.WindowsInstaller;
    using WixToolset.Extensibility;
    using WixToolset.Extensibility.Data;

    internal class ExampleWindowsInstallerBackendExtension : BaseWindowsInstallerBackendBinderExtension
    {
        public override IReadOnlyCollection<TableDefinition> TableDefinitions => ExampleTableDefinitions.All;

        /// <summary>
        /// Demonstrates <see cref="IWindowsInstallerBackendBinderExtension.PreProcessFiles"/>: files whose file
        /// name starts with "sign_" are redirected to a modified copy, leaving the original untouched.
        /// </summary>
        public override void PreProcessFiles(IntermediateSection section, IReadOnlyCollection<IFileFacade> files)
        {
            foreach (var file in files)
            {
                if (String.IsNullOrEmpty(file.SourcePath) || !Path.GetFileName(file.SourcePath).StartsWith("sign_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var folder = Path.Combine(this.Context.IntermediateFolder, "signed", file.Id);
                Directory.CreateDirectory(folder);

                var copy = Path.Combine(folder, Path.GetFileName(file.SourcePath));
                File.Copy(file.SourcePath, copy, overwrite: true);
                File.AppendAllText(copy, ExampleConstants.SignedMarker);

                file.SourcePath = copy;
            }
        }

        public override bool TryProcessSymbol(IntermediateSection section, IntermediateSymbol symbol, WindowsInstallerData output, TableDefinitionCollection tableDefinitions)
        {
            if (ExampleSymbolDefinitions.TryGetSymbolType(symbol.Definition.Name, out var symbolType))
            {
                switch (symbolType)
                {
                    case ExampleSymbolDefinitionType.Example:
                    {
                        var row = (ExampleRow)this.BackendHelper.CreateRow(section, symbol, output, ExampleTableDefinitions.ExampleTable);
                        row.Example = symbol.Id.Id;
                        row.ComponentRef = symbol[0].AsString();
                        row.Value = symbol[1].AsString();
                        return true;
                    }
                }
            }

            return base.TryProcessSymbol(section, symbol, output, tableDefinitions);
        }
    }
}

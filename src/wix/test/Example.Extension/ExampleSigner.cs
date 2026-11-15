// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using WixToolset.Extensibility.Data;

    /// <summary>
    /// Simulates signing for the PreProcessSourceFiles hooks: every "sign_*" file, except icons, is redirected
    /// to a copy in the intermediate folder with "SIGNED" appended, leaving the original untouched.
    /// </summary>
    internal static class ExampleSigner
    {
        public static void Sign(string intermediateFolder, IEnumerable<ISourceFile> files)
        {
            foreach (var file in files)
            {
                if (file.Kind == SourceFileKind.Icon || String.IsNullOrEmpty(file.SourcePath) || !Path.GetFileName(file.SourcePath).StartsWith("sign_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var folder = Path.Combine(intermediateFolder, "signed", file.Kind.ToString(), file.Id);
                Directory.CreateDirectory(folder);

                var copy = Path.Combine(folder, Path.GetFileName(file.SourcePath));
                File.Copy(file.SourcePath, copy, overwrite: true);
                File.AppendAllText(copy, ExampleConstants.SignedMarker);

                file.SourcePath = copy;
            }
        }
    }
}

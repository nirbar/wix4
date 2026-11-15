// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System.Xml.Linq;

    internal class ExampleConstants
    {
        public static readonly XNamespace Namespace = "http://www.example.com/scheams/v1/wxs";

        public static readonly XName ExampleName = Namespace + "Example";

        /// <summary>
        /// Appended by the pre-process hooks to the copy of every "sign_*" file, to simulate signing.
        /// </summary>
        public const string SignedMarker = "SIGNED";
    }
}

// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;

    /// <summary>
    /// Provides inline data for one iteration of a burn BA unit test.
    /// Apply multiple times to the same class to run multiple iterations on a single burn process.
    /// If no attribute is applied, the class runs once with an empty data array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class BurnBAInlineDataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BurnBAInlineDataAttribute"/> with the given data.
        /// </summary>
        public BurnBAInlineDataAttribute(params object[] data)
        {
            this.Data = data ?? Array.Empty<object>();
        }

        /// <summary>
        /// Gets the data for this iteration.
        /// </summary>
        public object[] Data { get; }
    }
}

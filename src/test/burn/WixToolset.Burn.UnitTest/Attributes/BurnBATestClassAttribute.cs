// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;

    /// <summary>
    /// Marks a class as a burn bootstrapper application unit test.
    /// Each class decorated with this attribute is treated as one test case by the MTP test framework.
    /// The class should inherit from <see cref="BurnBATestBase"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BurnBATestClassAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the overall timeout in seconds for this test class.
        /// Defaults to 600 (10 minutes) to allow for initial UX extraction.
        /// </summary>
        public int TimeoutSeconds { get; init; } = 600;
    }
}

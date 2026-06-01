// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides assertion methods for use inside burn BA unit tests.
    /// Failures throw <see cref="BurnBAAssertException"/>, which the test runner captures and
    /// reports as a test failure.
    /// </summary>
    public static class BurnAssert
    {
        /// <summary>
        /// Asserts that two values are equal.
        /// </summary>
        public static void Equal<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Fail(message ?? $"Expected: {Format(expected)}\nActual:   {Format(actual)}");
            }
        }

        /// <summary>
        /// Asserts that two values are not equal.
        /// </summary>
        public static void NotEqual<T>(T expected, T actual, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Fail(message ?? $"Expected values to differ, but both were: {Format(expected)}");
            }
        }

        /// <summary>
        /// Asserts that a condition is <see langword="true"/>.
        /// </summary>
        public static void True(bool condition, string message = null)
        {
            if (!condition)
            {
                Fail(message ?? "Expected condition to be true.");
            }
        }

        /// <summary>
        /// Asserts that a condition is <see langword="false"/>.
        /// </summary>
        public static void False(bool condition, string message = null)
        {
            if (condition)
            {
                Fail(message ?? "Expected condition to be false.");
            }
        }

        /// <summary>
        /// Asserts that a value is not <see langword="null"/>.
        /// </summary>
        public static void NotNull(object value, string message = null)
        {
            if (value == null)
            {
                Fail(message ?? "Expected a non-null value.");
            }
        }

        /// <summary>
        /// Asserts that a value is <see langword="null"/>.
        /// </summary>
        public static void Null(object value, string message = null)
        {
            if (value != null)
            {
                Fail(message ?? $"Expected null, but got: {Format(value)}");
            }
        }

        /// <summary>
        /// Asserts that a string contains a given substring.
        /// </summary>
        public static void Contains(string substring, string actual, string message = null)
        {
            if (actual == null || !actual.Contains(substring))
            {
                Fail(message ?? $"Expected string to contain '{substring}', but was: {Format(actual)}");
            }
        }

        /// <summary>
        /// Unconditionally fails with an optional message.
        /// </summary>
        public static void Fail(string message = null)
        {
            throw new BurnBAAssertException(message ?? "Assert.Fail was called.");
        }

        private static string Format(object value) => value is null ? "<null>" : value.ToString() ?? "<null>";
    }

    /// <summary>
    /// The exception thrown when a <see cref="BurnAssert"/> assertion fails.
    /// </summary>
    public sealed class BurnBAAssertException : Exception
    {
        /// <inheritdoc/>
        public BurnBAAssertException(string message) : base(message) { }

        /// <inheritdoc/>
        public BurnBAAssertException(string message, Exception innerException) : base(message, innerException) { }
    }
}

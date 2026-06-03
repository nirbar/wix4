// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Security.Principal;

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

        /// <summary>
        /// Gets or sets the global execution order for this test class.
        /// Tests with a lower <see cref="Order"/> value run first.
        /// Tests with the same order value run in discovery order.
        /// Defaults to 0.
        /// </summary>
        public int Order { get; init; } = 0;

        /// <summary>
        /// Gets or sets whether subsequent tests should be stopped when this test fails.
        /// When <see langword="true"/>, any test failure in this class causes all remaining
        /// tests in the same run to be reported as <c>Skipped</c>.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool StopTestsOnError { get; init; } = false;

        /// <summary>
        /// If set, skips this test with a message.
        /// </summary>
        public string Skip { get; set; } = null;

        /// <summary>
        /// If set, this test will run only if the process is executed with admin privileges
        /// </summary>
        public bool RequireAdmin { get; set; } = false;

        const string RequiredEnvironmentVariableName = "RuntimeTestsEnabled";
        public static bool RuntimeTestsEnabled { get; }
        public static bool RunningAsAdministrator { get; }

        static BurnBATestClassAttribute()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            RunningAsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);

            var testsEnabledString = Environment.GetEnvironmentVariable(RequiredEnvironmentVariableName);
            RuntimeTestsEnabled = Boolean.TryParse(testsEnabledString, out var testsEnabled) && testsEnabled;
        }

        public BurnBATestClassAttribute()
        {
            if (RequireAdmin && (!RuntimeTestsEnabled || !RunningAsAdministrator))
            {
                this.Skip = $"These tests must run elevated ({(RunningAsAdministrator ? "passed" : "failed")}). These tests affect machine state. To accept the consequences, set the {RequiredEnvironmentVariableName} environment variable to true ({(RuntimeTestsEnabled ? "passed" : "failed")}).";
            }
        }
    }
}

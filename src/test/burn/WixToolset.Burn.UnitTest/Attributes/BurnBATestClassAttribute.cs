// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Security.Principal;

    /// <summary>
    /// A <see cref="BurnBATestClassAttribute"/> specifying when to attach bundle logs to test results
    /// </summary>
    public enum AttachLogs
    {
        /// <summary>
        /// Always attach bundle logs to test results
        /// </summary>
        Always,
        /// <summary>
        /// Attach bundle logs to test results on test failures
        /// </summary>
        OnFailure,
        /// <summary>
        /// Never attach bundle logs to test results
        /// </summary>
        Never
    }

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
        /// Under what test results to attach bundle log files. Defaults to <see cref="AttachLogs.OnFailure"/>
        /// </summary>
        public AttachLogs AttachLogs {  get; init; } = AttachLogs.OnFailure;

        private string _Skip = null;
        /// <summary>
        /// If set, skips this test with a message.
        /// </summary>
        public string Skip
        {
            get
            {
                if (string.IsNullOrEmpty(_Skip) && RequireAdmin && (!RuntimeTestsEnabled || !RunningAsAdministrator))
                {
                    this.Skip = $"This test must run elevated and with environment variable '{nameof(RuntimeTestsEnabled)}' set to true.";
                }
                return _Skip;
            }
            set
            {
                _Skip = value;
            }
        }

        /// <summary>
        /// If set, this test will run only if the process is executed with admin privileges
        /// </summary>
        public bool RequireAdmin { get; set; } = false;

        public static bool RuntimeTestsEnabled { get; }
        public static bool RunningAsAdministrator { get; }

        static BurnBATestClassAttribute()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            RunningAsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);

            var testsEnabledString = Environment.GetEnvironmentVariable(nameof(RuntimeTestsEnabled));
            RuntimeTestsEnabled = Boolean.TryParse(testsEnabledString, out var testsEnabled) && testsEnabled;
        }
    }
}

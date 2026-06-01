// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using WixToolset.Burn.UnitTest.Internal;

    /// <summary>
    /// Provides a simple programmatic API for running burn BA unit tests from outside an MTP
    /// runner (e.g. from an xUnit <c>[Fact]</c> or <c>[RuntimeFact]</c> test method).
    ///
    /// Usage:
    /// <code>
    ///   await BurnBATestHost.RunAsync(bundlePath, password,
    ///       (typeof(MyInstallTest), Array.Empty&lt;object&gt;()));
    /// </code>
    /// If any test fails, an <see cref="AggregateException"/> is thrown containing the failure details.
    /// </summary>
    public static class BurnBATestHost
    {
        /// <summary>
        /// Runs one or more burn BA test classes against a bundle, in the order supplied.
        /// </summary>
        /// <param name="bundlePath">Path to the bundle executable to test.</param>
        /// <param name="password">
        /// Burn unittest handshake password (must match the value configured in the bundle's
        /// burn engine, e.g. via the <c>-burn.unittest</c> command-line switch).
        /// </param>
        /// <param name="tests">
        /// An ordered sequence of (test class type, per-iteration data) tuples.
        /// The types must derive from <see cref="BurnBATestBase"/> and have a public parameterless constructor.
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="AggregateException">Thrown if one or more tests fail.</exception>
        public static async Task RunAsync(
            string bundlePath,
            string password,
            IEnumerable<(Type TestType, object[] TestData)> tests,
            CancellationToken cancellationToken = default)
        {
            if (bundlePath == null)
            {
                throw new ArgumentNullException(nameof(bundlePath));
            }

            if (tests == null)
            {
                throw new ArgumentNullException(nameof(tests));
            }

            var entries = new List<TestRunEntry>();
            int index = 0;
            foreach (var (type, data) in tests)
            {
                if (!typeof(BurnBATestBase).IsAssignableFrom(type))
                {
                    throw new ArgumentException($"Type '{type.FullName}' does not derive from BurnBATestBase.", nameof(tests));
                }

                var fakeSource = Assembly.GetAssembly(type)?.Location ?? type.FullName!;
                var tc = new TestCase(type.FullName!, BurnBATestFrameworkExecutor.ExecutorUri, fakeSource);
                entries.Add(new TestRunEntry(tc, type, data ?? Array.Empty<object>(), index++));
            }

            if (entries.Count == 0)
            {
                return;
            }

            var handle = new CollectingFrameworkHandle();
            await BurnBATestRunner.RunAllAsync(entries, bundlePath, password, handle, cancellationToken)
                .ConfigureAwait(false);

            var failures = handle.Results
                .Where(r => r.Outcome == TestOutcome.Failed)
                .ToList();

            if (failures.Count > 0)
            {
                var exceptions = failures.Select(r =>
                    new Exception($"Test '{r.TestCase.FullyQualifiedName}' failed: {r.ErrorMessage}\n{r.ErrorStackTrace}"));
                throw new AggregateException("One or more burn BA unit tests failed.", exceptions);
            }
        }

        /// <summary>
        /// Convenience overload for running a single test class with no inline data.
        /// </summary>
        public static Task RunAsync<TTest>(
            string bundlePath,
            string password,
            CancellationToken cancellationToken = default)
            where TTest : BurnBATestBase, new()
        {
            return RunAsync(bundlePath, password,
                new[] { (typeof(TTest), Array.Empty<object>()) },
                cancellationToken);
        }

        // -----------------------------------------------------------------------
        // Minimal IFrameworkHandle implementation that just collects results.
        // -----------------------------------------------------------------------

        private sealed class CollectingFrameworkHandle : IFrameworkHandle
        {
            internal List<TestResult> Results { get; } = new List<TestResult>();

            public bool EnableShutdownAfterTestRun { get; set; }

            public void RecordResult(TestResult testResult) => this.Results.Add(testResult);

            public void RecordStart(TestCase testCase) { }

            public void RecordEnd(TestCase testCase, TestOutcome outcome) { }

            public void RecordAttachments(IList<AttachmentSet> attachmentSets) { }

            public void SendMessage(TestMessageLevel testMessageLevel, string message) { }

            public int LaunchProcessWithDebuggerAttached(
                string filePath, string workingDirectory, string arguments,
                IDictionary<string, string> environmentVariables)
            {
                throw new NotSupportedException("LaunchProcessWithDebuggerAttached is not supported in standalone mode.");
            }
        }
    }
}

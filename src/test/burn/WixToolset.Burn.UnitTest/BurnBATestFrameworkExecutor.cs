// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using WixToolset.Burn.UnitTest.Internal;

    /// <summary>
    /// MTP test executor for burn BA unit tests.
    /// Groups the provided test cases by source assembly, resolves their types and inline data,
    /// and runs them all in a single burn process per source assembly.
    /// </summary>
    [ExtensionUri(ExecutorUriString)]
    public sealed class BurnBATestFrameworkExecutor : ITestExecutor
    {
        internal const string ExecutorUriString = "executor://BurnBATestFramework/v1";

        /// <summary>
        /// The executor URI used in <see cref="DefaultExecutorUriAttribute"/> and <see cref="ExtensionUriAttribute"/>.
        /// </summary>
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);

        private CancellationTokenSource _cts;

        /// <inheritdoc/>
        // To debug, set environment variable VSTEST_HOST_DEBUG=1 before launching `dotnet test`
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (sources == null || frameworkHandle == null)
            {
                return;
            }

            var settings = BurnBATestFrameworkRunSettings.Load(runContext?.RunSettings);

            foreach (var source in sources)
            {
                var testCases = DiscoverTestCasesFromSource(source);
                this.RunTestCasesFromSource(source, testCases, settings, frameworkHandle);
            }
        }

        /// <inheritdoc/>
        // To debug, set environment variable VSTEST_HOST_DEBUG=1 before launching `dotnet test`
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (tests == null || frameworkHandle == null)
            {
                return;
            }

            var settings = BurnBATestFrameworkRunSettings.Load(runContext?.RunSettings);

            // Group by source assembly so we can do one burn launch per assembly.
            var bySource = tests.GroupBy(t => t.Source);
            foreach (var group in bySource)
            {
                this.RunTestCasesFromSource(group.Key, group, settings, frameworkHandle);
            }
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            this._cts?.Cancel();
        }

        private void RunTestCasesFromSource(
            string source,
            IEnumerable<TestCase> testCases,
            BurnBATestFrameworkRunSettings settings,
            IFrameworkHandle frameworkHandle)
        {
            if (string.IsNullOrEmpty(settings.BundlePath))
            {
                foreach (var tc in testCases)
                {
                    frameworkHandle.RecordStart(tc);
                    var result = new TestResult(tc)
                    {
                        Outcome = TestOutcome.Failed,
                        ErrorMessage = "BundlePath is not configured.  Add <BurnBATestFramework><BundlePath>...</BundlePath></BurnBATestFramework> to your .runsettings file.",
                    };
                    frameworkHandle.RecordResult(result);
                }
                return;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(source);
            }
            catch (Exception ex)
            {
                foreach (var tc in testCases)
                {
                    frameworkHandle.RecordStart(tc);
                    var result = new TestResult(tc)
                    {
                        Outcome = TestOutcome.Failed,
                        ErrorMessage = $"Failed to load assembly '{source}': {ex.Message}",
                    };
                    frameworkHandle.RecordResult(result);
                }
                return;
            }

            // Build the globally-ordered list of entries.
            var entries = new List<TestRunEntry>();
            foreach (var testCase in testCases)
            {
                frameworkHandle.RecordStart(testCase);

                var className = testCase.GetPropertyValue<string>(BurnBATestFrameworkDiscoverer.TestClassNameProperty, null);
                var iterationIndex = testCase.GetPropertyValue<int>(BurnBATestFrameworkDiscoverer.IterationIndexProperty, 0);
                var order = testCase.GetPropertyValue<int>(BurnBATestFrameworkDiscoverer.OrderProperty, 0);
                var stopTestsOnError = testCase.GetPropertyValue<bool>(BurnBATestFrameworkDiscoverer.StopTestsOnErrorProperty, false);

                if (string.IsNullOrEmpty(className))
                {
                    var result = new TestResult(testCase)
                    {
                        Outcome = TestOutcome.Failed,
                        ErrorMessage = "Test case is missing BurnBA.ClassName property.",
                    };
                    frameworkHandle.RecordResult(result);
                    continue;
                }

                var type = assembly.GetType(className);
                if (type == null)
                {
                    var result = new TestResult(testCase)
                    {
                        Outcome = TestOutcome.Failed,
                        ErrorMessage = $"Type '{className}' not found in assembly '{source}'.",
                    };
                    frameworkHandle.RecordResult(result);
                    continue;
                }

                // Resolve inline data for this iteration.
                var inlineDataAttrs = type.GetCustomAttributes<BurnBAInlineDataAttribute>(inherit: false).ToList();
                var testData = inlineDataAttrs.Count > iterationIndex
                    ? inlineDataAttrs[iterationIndex].Data
                    : Array.Empty<object>();

                entries.Add(new TestRunEntry(testCase, type, testData, iterationIndex, stopTestsOnError));
            }

            if (entries.Count == 0)
            {
                return;
            }

            // Sort by the Order property from the attribute (stable sort preserves discovery order for ties).
            var orderedEntries = entries
                .Select((e, idx) => (entry: e, idx, order: e.TestCase.GetPropertyValue<int>(BurnBATestFrameworkDiscoverer.OrderProperty, 0)))
                .OrderBy(t => t.order)
                .ThenBy(t => t.idx)
                .Select(t => t.entry)
                .ToList();

            using var cts = new CancellationTokenSource();
            this._cts = cts;

            try
            {
                BurnBATestRunner.RunAllAsync(orderedEntries, settings.BundlePath, settings.Password, frameworkHandle, cts.Token)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (OperationCanceledException)
            {
                // Cancelled externally; individual results are recorded in the runner.
            }
            catch (Exception ex)
            {
                // Unexpected runner failure: report against all remaining not-yet-completed tests.
                foreach (var entry in orderedEntries)
                {
                    var result = new TestResult(entry.TestCase)
                    {
                        Outcome = TestOutcome.Failed,
                        ErrorMessage = $"BurnBATestRunner failed: {ex.Message}",
                        ErrorStackTrace = ex.StackTrace,
                    };
                    frameworkHandle.RecordResult(result);
                }
            }
            finally
            {
                this._cts = null;
            }
        }

        private static List<TestCase> DiscoverTestCasesFromSource(string source)
        {
            var testCases = new List<TestCase>();
            var discoverer = new BurnBATestFrameworkDiscoverer();

            var sink = new CollectingDiscoverySink(testCases);
            discoverer.DiscoverTests(
                new[] { source },
                discoveryContext: null!,
                logger: NullMessageLogger.Instance,
                discoverySink: sink);

            return testCases;
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private sealed class CollectingDiscoverySink : ITestCaseDiscoverySink
        {
            private readonly List<TestCase> _testCases;

            internal CollectingDiscoverySink(List<TestCase> testCases)
            {
                this._testCases = testCases;
            }

            public void SendTestCase(TestCase discoveredTest)
            {
                this._testCases.Add(discoveredTest);
            }
        }

        private sealed class NullMessageLogger : IMessageLogger
        {
            internal static readonly NullMessageLogger Instance = new NullMessageLogger();

            public void SendMessage(TestMessageLevel testMessageLevel, string message) { }
        }
    }
}

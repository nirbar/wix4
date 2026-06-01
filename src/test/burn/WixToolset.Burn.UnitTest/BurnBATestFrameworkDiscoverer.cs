// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using WixToolset.Burn.UnitTest.Internal;

    /// <summary>
    /// MTP test discoverer for burn BA unit tests.
    /// Scans assemblies for classes decorated with <see cref="BurnBATestClassAttribute"/> and
    /// emits one <see cref="TestCase"/> per class (or one per <see cref="BurnBAInlineDataAttribute"/>
    /// when multiple inline data attributes are applied).
    /// </summary>
    [DefaultExecutorUri(BurnBATestFrameworkExecutor.ExecutorUriString)]
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    public sealed class BurnBATestFrameworkDiscoverer : ITestDiscoverer
    {
        /// <summary>
        /// TestProperty that stores the 0-based inline-data iteration index for this test case.
        /// </summary>
        public static readonly TestProperty IterationIndexProperty =
            TestProperty.Register(
                "BurnBA.IterationIndex",
                "Burn BA Iteration Index",
                typeof(int),
                typeof(BurnBATestFrameworkDiscoverer));

        /// <summary>
        /// TestProperty that stores the fully-qualified type name of the test class.
        /// </summary>
        public static readonly TestProperty TestClassNameProperty =
            TestProperty.Register(
                "BurnBA.ClassName",
                "Burn BA Test Class Name",
                typeof(string),
                typeof(BurnBATestFrameworkDiscoverer));

        /// <inheritdoc/>
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            foreach (var source in sources)
            {
                try
                {
                    DiscoverAssembly(source, discoverySink);
                }
                catch (Exception ex)
                {
                    logger.SendMessage(TestMessageLevel.Warning,
                        $"BurnBATestFramework: Failed to inspect '{source}': {ex.Message}");
                }
            }
        }

        private static void DiscoverAssembly(string assemblyPath, ITestCaseDiscoverySink discoverySink)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch
            {
                return;
            }

            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                var classAttr = type.GetCustomAttribute<BurnBATestClassAttribute>(inherit: false);
                if (classAttr == null)
                {
                    continue;
                }

                if (!typeof(BurnBATestBase).IsAssignableFrom(type))
                {
                    continue;
                }

                var inlineDataAttrs = type.GetCustomAttributes<BurnBAInlineDataAttribute>(inherit: false);
                var iterationList = new List<BurnBAInlineDataAttribute>(inlineDataAttrs);

                if (iterationList.Count == 0)
                {
                    // Single iteration with no data.
                    var testCase = MakeTestCase(assemblyPath, type, iterationIndex: 0, displaySuffix: null);
                    discoverySink.SendTestCase(testCase);
                }
                else
                {
                    for (int i = 0; i < iterationList.Count; i++)
                    {
                        var suffix = $"[{i}]";
                        var testCase = MakeTestCase(assemblyPath, type, iterationIndex: i, displaySuffix: suffix);
                        discoverySink.SendTestCase(testCase);
                    }
                }
            }
        }

        private static TestCase MakeTestCase(string source, Type type, int iterationIndex, string displaySuffix)
        {
            var fullyQualifiedName = type.FullName + displaySuffix;
            var testCase = new TestCase(fullyQualifiedName, BurnBATestFrameworkExecutor.ExecutorUri, source)
            {
                DisplayName = type.Name + displaySuffix,
            };
            testCase.SetPropertyValue(IterationIndexProperty, iterationIndex);
            testCase.SetPropertyValue(TestClassNameProperty, type.FullName!);
            return testCase;
        }
    }
}

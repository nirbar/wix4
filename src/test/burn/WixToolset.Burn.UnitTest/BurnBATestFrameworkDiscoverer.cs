namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// TestProperty that stores the name of the test class.
        /// </summary>
        public static readonly TestProperty ClassNameProperty =
            TestProperty.Register(
                "BurnBA.ClassName",
                "Burn BA Test Class Name",
                typeof(string),
                typeof(BurnBATestFrameworkDiscoverer));

        /// <summary>
        /// TestProperty that stores the fully-qualified type name of the test class.
        /// </summary>
        public static readonly TestProperty FullyQualifiedNameProperty =
            TestProperty.Register(
                "BurnBA.FullyQualifiedName",
                "Burn BA Test Fully Qualified Name",
                typeof(string),
                typeof(BurnBATestFrameworkDiscoverer));

        /// <summary>
        /// TestProperty that stores the <see cref="BurnBATestClassAttribute.Order"/> value.
        /// </summary>
        public static readonly TestProperty OrderProperty =
            TestProperty.Register(
                "BurnBA.Order",
                "Burn BA Test Order",
                typeof(int),
                typeof(BurnBATestFrameworkDiscoverer));

        /// <summary>
        /// TestProperty that stores the <see cref="BurnBATestClassAttribute.StopTestsOnError"/> value.
        /// </summary>
        public static readonly TestProperty StopTestsOnErrorProperty =
            TestProperty.Register(
                "BurnBA.StopTestsOnError",
                "Burn BA Stop Tests On Error",
                typeof(bool),
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
                    var testCase = MakeTestCase(assemblyPath, type, classAttr, iterationIndex: 0, displaySuffix: null);
                    discoverySink.SendTestCase(testCase);
                }
                else
                {
                    for (int i = 0; i < iterationList.Count; i++)
                    {
                        var suffix = $"[{i}]";
                        var testCase = MakeTestCase(assemblyPath, type, classAttr, iterationIndex: i, displaySuffix: suffix);
                        discoverySink.SendTestCase(testCase);
                    }
                }
            }
        }

        private static TestCase MakeTestCase(string source, Type type, BurnBATestClassAttribute classAttr, int iterationIndex, string displaySuffix)
        {
            var fullyQualifiedName = type.FullName + displaySuffix;
            var testCase = new TestCase(fullyQualifiedName, BurnBATestFrameworkExecutor.ExecutorUri, source)
            {
                DisplayName = type.Name + displaySuffix,
            };
            testCase.SetPropertyValue(IterationIndexProperty, iterationIndex);
            testCase.SetPropertyValue(FullyQualifiedNameProperty, type.FullName!);
            testCase.SetPropertyValue(ClassNameProperty, type.Name!);
            testCase.SetPropertyValue(OrderProperty, classAttr.Order);
            testCase.SetPropertyValue(StopTestsOnErrorProperty, classAttr.StopTestsOnError);
            return testCase;
        }

        internal static TestCase MakeLastTestCase()
        {
            var type = typeof(LastTest);
            var fullyQualifiedName = type.FullName;
            var testCase = new TestCase(fullyQualifiedName, BurnBATestFrameworkExecutor.ExecutorUri, fullyQualifiedName)
            {
                DisplayName = type.Name,
            };
            testCase.SetPropertyValue(IterationIndexProperty, 0);
            testCase.SetPropertyValue(FullyQualifiedNameProperty, type.FullName!);
            testCase.SetPropertyValue(ClassNameProperty, type.Name!);
            testCase.SetPropertyValue(OrderProperty, Int32.MaxValue);
            testCase.SetPropertyValue(StopTestsOnErrorProperty, false);
            return testCase;
        }

        internal static object GetTestCasePropertyValue(TestCase testCase, string propertyId)
        {
            var prop = testCase.Properties.FirstOrDefault(p => (p.Id.Equals(propertyId) || p.Id.Equals("BurnBA." + propertyId)));
            if (prop == null)
            {
                return null;
            }

            return testCase.GetPropertyValue(prop);
        }

        internal static TestProperty GetTestCaseProperty(string propertyId)
        {
            switch (propertyId)
            {
                case "IterationIndex":
                case "BurnBA.IterationIndex":
                    return IterationIndexProperty;
                case "FullyQualifiedName":
                case "BurnBA.FullyQualifiedName":
                    return FullyQualifiedNameProperty;
                case "ClassName":
                case "BurnBA.IterClassNameationIndex":
                    return ClassNameProperty;
                case "Order":
                case "BurnBA.Order":
                    return OrderProperty;
                case "StopTestsOnError":
                case "BurnBA.StopTestsOnError":
                    return StopTestsOnErrorProperty;
                default:
                    return null;
            }
        }

        internal static IEnumerable<string> GetSupportedProperties()
        {
            return new string[]
            {
                "IterationIndex",
                "FullyQualifiedName",
                "ClassName",
                "Order",
                "StopTestsOnError",

                "BurnBA.IterationIndex",
                "BurnBA.FullyQualifiedName",
                "BurnBA.ClassName",
                "BurnBA.Order",
                "BurnBA.StopTestsOnError",
            };
        }
    }
}

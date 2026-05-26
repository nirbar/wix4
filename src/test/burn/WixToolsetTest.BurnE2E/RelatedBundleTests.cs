// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.BurnE2E
{
    using WixTestTools;
    using Xunit;

    public class RelatedBundleTests : BurnE2ETests
    {
        public RelatedBundleTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [RuntimeFact]
        public void CanUninstallRelatedBundle()
        {
            /* 
             * BundleA installs PackageA
             * BundleB installs PackageB, and removes BundleA- both on install & uninstall
             */

            var packageA = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageA");
            var packageB = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageB");
            var bundleA = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleA");
            var bundleB = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleB");

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);

            bundleB.Install();
            bundleB.VerifyRegisteredAndInPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(false);
            packageB.VerifyInstalled(true);

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            bundleB.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);
            packageB.VerifyInstalled(true);

            bundleB.Uninstall();
            bundleB.VerifyUnregisteredAndRemovedFromPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(false);
            packageB.VerifyInstalled(false);
        }

        [RuntimeFact]
        public void IgnoreUninstallRelatedBundleAsChainPackage()
        {
            /* 
             * BundleA installs PackageA
             * BundleC:
             *  - PackageB
             *  - BundleA as an uninstall relatedbundle, and also as a chain package 
             */

            var packageA = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageA");
            var packageB = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageB");
            var bundleA = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleA");
            var bundleC = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleC");

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);

            var logFile = bundleC.Install();
            bundleC.VerifyRegisteredAndInPackageCache();
            bundleA.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);
            packageB.VerifyInstalled(true);
            Assert.True(LogVerifier.MessageInLogFileRegex(logFile, "Plan skipped related bundle: .*, because it is also a bundle package"), "Expected log message");

            logFile = bundleC.Uninstall();
            bundleC.VerifyUnregisteredAndRemovedFromPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(false);
            packageB.VerifyInstalled(false);
            Assert.True(LogVerifier.MessageInLogFileRegex(logFile, "Plan skipped related bundle: .*, because it is also a bundle package"), "Expected log message");
        }

        [RuntimeFact]
        public void IgnoreUninstallRelatedBundleWithUpgradeChainPackage()
        {
            /* 
             * BundleA installs PackageA
             * BundleAv2 installs PackageA, has version 2.0.0.0
             * BundleE:
             *  - PackageB
             *  - BundleAv2 as an uninstall relatedbundle, and also as a chain package
             */

            var packageA = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageA");
            var packageB = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageB");
            var bundleA = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleA");
            var bundleAv2 = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleAv2");
            var BundleE = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleE");

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);

            var logFile = BundleE.Install();
            BundleE.VerifyRegisteredAndInPackageCache();
            bundleAv2.VerifyRegisteredAndInPackageCache(1);
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(true);
            packageB.VerifyInstalled(true);
            Assert.True(LogVerifier.MessageInLogFileRegex(logFile, "Skipped related uninstall bundle: .* since it is already absent, action: Uninstall"), "Expected log message");

            logFile = BundleE.Uninstall();
            BundleE.VerifyUnregisteredAndRemovedFromPackageCache();
            bundleAv2.VerifyUnregisteredAndRemovedFromPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(false);
            packageB.VerifyInstalled(false);
            Assert.True(LogVerifier.MessageInLogFileRegex(logFile, "Plan skipped related bundle: .*, because it is also a bundle package"), "Expected log message");
        }

        [RuntimeFact]
        public void CanUninstallRelatedBundleWithCommonPackage()
        {
            /* 
             * BundleA installs PackageA
             * BundleD installs PackageA, and removes BundleA- both on install & uninstall
             */

            var packageA = this.CreatePackageInstaller(@"..\RelatedBundleTests\PackageA");
            var bundleA = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleA");
            var bundleD = this.CreateBundleInstaller(@"..\RelatedBundleTests\BundleD");

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);

            bundleD.Install();
            bundleD.VerifyRegisteredAndInPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(true);

            bundleA.Install();
            bundleA.VerifyRegisteredAndInPackageCache();
            bundleD.VerifyRegisteredAndInPackageCache();
            packageA.VerifyInstalled(true);

            bundleD.Uninstall();
            bundleD.VerifyUnregisteredAndRemovedFromPackageCache();
            bundleA.VerifyUnregisteredAndRemovedFromPackageCache();
            packageA.VerifyInstalled(false);
        }
    }
}

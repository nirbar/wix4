// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.TestTools
{
    using System;
    using System.Linq;
    using MsiZapEx;

    /// <summary>
    /// Removes orphaned MSI / bundle registration entries left behind by failed or
    /// incomplete installations using the MsiZapEx library.
    /// </summary>
    public class MsiZapCleaner
    {
        private readonly Xunit.ITestOutputHelper outputHelper;

        public MsiZapCleaner(Xunit.ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        /// <summary>
        /// Removes all registry / cache entries associated with the given MSI product code.
        /// </summary>
        /// <param name="productCode">MSI product code GUID (with or without braces).</param>
        public void CleanProductCode(string productCode)
        {
            if (String.IsNullOrWhiteSpace(productCode))
            {
                throw new ArgumentNullException(nameof(productCode));
            }

            if (!Guid.TryParse(productCode, out var productGuid))
            {
                throw new ArgumentException($"'{productCode}' is not a valid GUID.", nameof(productCode));
            }

            try
            {
                var upgradeInfo = UpgradeInfo.FindByProductCode(productGuid, shallow: true);
                if (upgradeInfo == null)
                {
                    this.outputHelper.WriteLine($"MsiZapCleaner: product {productCode} not found, nothing to clean.");
                    return;
                }

                var product = upgradeInfo.RelatedProducts.FirstOrDefault(p => p.ProductCode.Equals(productGuid));
                if (product == null)
                {
                    this.outputHelper.WriteLine($"MsiZapCleaner: product {productCode} not found in upgrade info, nothing to clean.");
                    return;
                }

                upgradeInfo.Prune(product);
                this.outputHelper.WriteLine($"MsiZapCleaner: cleaned product {productCode}");
            }
            catch (Exception ex)
            {
                this.outputHelper.WriteLine($"MsiZapCleaner: warning — could not clean product {productCode}: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all registry / cache entries associated with bundles registered under the given upgrade code.
        /// </summary>
        /// <param name="bundleUpgradeCode">Bundle upgrade code GUID (with or without braces).</param>
        public void CleanBundleUpgradeCode(string bundleUpgradeCode)
        {
            if (String.IsNullOrWhiteSpace(bundleUpgradeCode))
            {
                throw new ArgumentNullException(nameof(bundleUpgradeCode));
            }

            if (!Guid.TryParse(bundleUpgradeCode, out var upgradeGuid))
            {
                throw new ArgumentException($"'{bundleUpgradeCode}' is not a valid GUID.", nameof(bundleUpgradeCode));
            }

            try
            {
                var bundles = BundleInfo.FindByUpgradeCode(upgradeGuid);
                if (bundles == null || bundles.Count == 0)
                {
                    this.outputHelper.WriteLine($"MsiZapCleaner: no bundles found for upgrade code {bundleUpgradeCode}, nothing to clean.");
                    return;
                }

                foreach (var bundle in bundles)
                {
                    bundle.Prune();
                    this.outputHelper.WriteLine($"MsiZapCleaner: cleaned bundle {bundle.BundleProductCode} (upgrade code {bundleUpgradeCode})");
                }
            }
            catch (Exception ex)
            {
                this.outputHelper.WriteLine($"MsiZapCleaner: warning — could not clean bundle {bundleUpgradeCode}: {ex.Message}");
            }
        }
    }
}

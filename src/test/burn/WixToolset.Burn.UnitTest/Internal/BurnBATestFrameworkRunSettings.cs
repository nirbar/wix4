// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// Reads BurnBATestFramework-specific RunSettings from a dotnet-test RunSettings XML document.
    /// Expected XML shape:
    /// <code>
    /// &lt;RunSettings&gt;
    ///   &lt;BurnBATestFramework&gt;
    ///     &lt;BundlePath&gt;C:\path\to\bundle.exe&lt;/BundlePath&gt;
    ///     &lt;Password&gt;plaintextpassword&lt;/Password&gt;
    ///   &lt;/BurnBATestFramework&gt;
    /// &lt;/RunSettings&gt;
    /// </code>
    /// </summary>
    internal sealed class BurnBATestFrameworkRunSettings
    {
        internal string BundlePath { get; private set; } = string.Empty;
        internal string Password { get; private set; } = string.Empty;

        internal static BurnBATestFrameworkRunSettings Load(IRunSettings runSettings)
        {
            var result = new BurnBATestFrameworkRunSettings();
            if (runSettings?.SettingsXml == null)
            {
                return result;
            }

            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(runSettings.SettingsXml);

                var bundlePathNode = doc.SelectSingleNode("/RunSettings/BurnBATestFramework/BundlePath");
                if (bundlePathNode != null)
                {
                    result.BundlePath = bundlePathNode.InnerText.Trim();
                }

                var passwordNode = doc.SelectSingleNode("/RunSettings/BurnBATestFramework/Password");
                if (passwordNode != null)
                {
                    result.Password = passwordNode.InnerText.Trim();
                }
            }
            catch
            {
                // If settings parsing fails, return empty/defaults.
            }

            return result;
        }
    }
}

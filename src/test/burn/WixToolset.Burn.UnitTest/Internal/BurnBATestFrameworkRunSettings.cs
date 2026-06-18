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

                // TestRunParameters override the section values, allowing dotnet test command-line
                //   dotnet test -- TestRunParameters.Parameter(name=\"BundlePath\",value=\"C:\path.exe\")
                var bundlePathParam = doc.SelectSingleNode("/RunSettings/TestRunParameters/Parameter[@name='BundlePath']");
                if (bundlePathParam?.Attributes?["value"]?.Value?.Trim() is { Length: > 0 } bp)
                {
                    result.BundlePath = bp;
                }

                var passwordParam = doc.SelectSingleNode("/RunSettings/TestRunParameters/Parameter[@name='Password']");
                if (passwordParam?.Attributes?["value"]?.Value?.Trim() is { Length: > 0 } pw)
                {
                    result.Password = pw;
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

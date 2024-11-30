namespace UpdateManager.CoreLibrary.GeneralHelpers.NugetHelpers;
internal static class NuGetFeedConfiguration
{
    // Private helper method to load the NuGet config file
    private static XDocument LoadNuGetConfig()
    {
        string nugetConfigPath = bb1.Configuration!.GetNugetConfigPath();
        if (!File.Exists(nugetConfigPath))
        {
            throw new CustomBasicException("NuGet feed config file does not exist.");
        }
        return XDocument.Load(nugetConfigPath);
    }

    // Add a temporary feed to the NuGet config
    public static bool AddTempFeed(string feedKey, string feedPath)
    {
        var config = LoadNuGetConfig();
        var packageSources = config.Element("configuration")?.Element("packageSources");
        if (packageSources != null)
        {
            var existingFeed = packageSources.Elements("add")
                                             .FirstOrDefault(x => (string)x.Attribute("key")! == feedKey);
            if (existingFeed == null)
            {
                XElement newFeedElement = new("add",
                    new XAttribute("key", feedKey),
                    new XAttribute("value", feedPath)
                );
                packageSources.AddFirst(newFeedElement);
                config.Save(bb1.Configuration!.GetNugetConfigPath());
                return true; // Feed successfully added
            }
        }
        return false; // Feed already exists
    }

    // Remove a temporary feed from the NuGet config
    public static bool RemoveTempFeed(string feedKey)
    {
        var config = LoadNuGetConfig();
        var packageSources = config.Element("configuration")?.Element("packageSources");

        if (packageSources != null)
        {
            var tempFeed = packageSources.Elements("add")
                                         .FirstOrDefault(e => (string)e.Attribute("key")! == feedKey);
            if (tempFeed != null)
            {
                tempFeed.Remove();
                config.Save(bb1.Configuration!.GetNugetConfigPath());
                return true; // Feed successfully removed
            }
        }
        return false; // Feed not found
    }
}
namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetFeedConfiguration
{
    // Private helper method to load the NuGet config file
    private static XDocument LoadNuGetConfig()
    {
        string nugetConfigPath = bb1.Configuration!.NugetConfigPath;
        if (!File.Exists(nugetConfigPath))
        {
            throw new CustomBasicException("NuGet feed config file does not exist.");
        }
        return XDocument.Load(nugetConfigPath);
    }

    // Add a permanent feed (this will never be removed)
    public static bool AddPermanentFeed(string feedKey, string feedPath)
    {
        // Call the regular AddFeed method with permanent flag set to true
        return AddFeed(feedKey, feedPath, permanent: true);
    }

    // Add a temporary feed (this can be removed later)
    public static bool AddTemporaryFeed(string feedKey, string feedPath)
    {
        // Call the regular AddFeed method with permanent flag set to false
        return AddFeed(feedKey, feedPath, permanent: false);
    }

    // Add a feed (temp or permanent) to the NuGet config
    private static bool AddFeed(string feedKey, string feedPath, bool permanent = false)
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

                // If permanent is true, add the permanent attribute
                if (permanent)
                {
                    newFeedElement.SetAttributeValue("permanent", "true");
                }

                packageSources.AddFirst(newFeedElement);  // Insert at the top (highest priority)
                config.Save(bb1.Configuration!.NugetConfigPath);
                return true;  // Feed successfully added
            }
        }
        return false;  // Feed already exists
    }

    // Remove a feed from the NuGet config (throw error if permanent)
    public static bool RemoveFeed(string feedKey)
    {
        var config = LoadNuGetConfig();
        var packageSources = config.Element("configuration")?.Element("packageSources");

        if (packageSources != null)
        {
            var feedToRemove = packageSources.Elements("add")
                .FirstOrDefault(e => (string)e.Attribute("key")! == feedKey);

            if (feedToRemove != null)
            {
                // Check if the feed is permanent (i.e., has the "permanent" attribute)
                if (feedToRemove.Attribute("permanent")?.Value == "true")
                {
                    Console.WriteLine($"Error: The feed '{feedKey}' is permanent and cannot be removed.");
                    return false;  // Cannot remove permanent feed
                }

                // Remove the feed if it's not permanent
                feedToRemove.Remove();
                config.Save(bb1.Configuration!.NugetConfigPath);
                return true;  // Feed successfully removed
            }
        }
        return false;  // Feed not found
    }
}
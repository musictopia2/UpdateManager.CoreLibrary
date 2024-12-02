namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class YearlyFeedManager(INugetPacker packer) : IYearlyFeedManager
{
    private static string TestLocalKey => "TestLocal";
    private static string TestPublicKey => "TestPublic";
    public static void ForceRemoveTestFeeds()
    {
        NuGetFeedConfiguration.RemoveFeed(TestLocalKey);
        NuGetFeedConfiguration.RemoveFeed(TestPublicKey); //if production, then can't even have those feeds anymore
    }
    void IYearlyFeedManager.CleanupYearlyFeed(string feedPath)
    {
        if (ff1.DirectoryExists(feedPath))
        {
            ff1.DeleteFolder(feedPath);
        }
        ff1.CreateFolder(feedPath); //go ahead and create folder (to plan for future).
    }
    void IYearlyFeedManager.ClearYearlyFeed(string feedPath)
    {
        if (ff1.DirectoryExists(feedPath))
        {
            ff1.DeleteFolder(feedPath);
        }
        ff1.CreateFolder(feedPath);
    }

    void IYearlyFeedManager.InitializeYearlyFeeds(DotNetVersionUpgradeModel upgradeModel)
    {
        if (upgradeModel.IsTestMode)
        {
            throw new CustomBasicException("Cannot be in test mode because this supports production only");
        }
        try
        {
            ForceRemoveTestFeeds(); // Make sure to clean up any test feeds
            // Clean up test-related folders
            if (ff1.DirectoryExists(upgradeModel.TestLocalFeedPath))
            {
                ff1.DeleteFolder(upgradeModel.TestLocalFeedPath);
            }
            if (ff1.DirectoryExists(upgradeModel.TestPublicFeedPath))
            {
                ff1.DeleteFolder(upgradeModel.TestPublicFeedPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing feeds: {ex.Message}");
            throw; // Re-throw the exception to notify the caller
        }
    }
    async Task<bool> IYearlyFeedManager.PublishPackageToYearlyFeedAsync(LibraryNetUpdateModel upgradeModel, string feedPath, CancellationToken cancellationToken)
    {
        bool rets = await packer.CreateNugetPackageAsync(upgradeModel, true, cancellationToken);
        if (rets == false)
        {
            return false;
        }
        var files = ff1.FileList(upgradeModel.NugetPackagePath);
        // Remove all files that do not end with ".nupkg" (case-insensitive)
        files.RemoveAllOnly(x => !x.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase));
        // Check if there's exactly one .nupkg file in the list
        if (files.Count != 1)
        {
            Console.WriteLine($"Error: Expected 1 .nupkg file, but found {files.Count}.");
            return false;
        }
        string nugetFile = files.Single();
        nugetFile = ff1.FullFile(nugetFile);
        rets = await PrivateNuGetFeedUploader.UploadPrivateNugetPackageAsync(feedPath, upgradeModel.NugetPackagePath, nugetFile, cancellationToken);
        return rets;
    }
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IYearlyFeedManager
{
    void InitializeYearlyFeeds(DotNetVersionUpgradeModel upgradeModel); // Initialize feeds for yearly updates, skip if already present
    Task<bool> PublishPackageToYearlyFeedAsync(LibraryNetUpdateModel upgradeModel, string feedPath, CancellationToken cancellationToken = default);  // Publish to a specialized feed
    void CleanupYearlyFeed(string feedPath); // Cleanup temporary feeds for the yearly update
    void ClearYearlyFeed(string feedPath); // Clear feed before testing (for the yearly process)
}
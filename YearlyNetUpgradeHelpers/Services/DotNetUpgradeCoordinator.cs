namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class DotNetUpgradeCoordinator(
    IFeedPathResolver feedPathResolver,
    IDotNetVersionInfoRepository dotNetVersionInfoManager,
    IPreUpgradeProcessHandler preUpgradeProcessHandler,
    INetVersionUpdateContext netVersionUpdateContext,
    ILibraryDotNetUpgraderBuild libraryDotNetUpgraderBuild,
    IYearlyFeedManager yearlyFeedManager,
    ILibraryDotNetUpgradeCommitter libraryDotNetUpgradeCommitter,
    IPostBuildCommandStrategy postBuildCommandStrategy
    )
{
    public async Task<UpgradeProcessState> GetUpgradeStatusAsync(BasicList<LibraryNetUpdateModel> libraries)
    {
        DotNetVersionUpgradeModel netUpgrade = await dotNetVersionInfoManager.GetVersionInfoAsync();

    }
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class DotNetUpgradeCoordinator(
    IFeedPathResolver feedPathResolver,
    IDateOnlyPicker picker,
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
        DotNetVersionUpgradeModel netConfig = await dotNetVersionInfoManager.GetVersionInfoAsync();
        bool needsUpdate;
        needsUpdate = netConfig.NeedsToUpdateVersion(picker);
        if (needsUpdate)
        {
            return new(EnumUpgradePhase.PendingUpdate, netConfig);
        }
        if (libraries.All(x => x.Status == EnumDotNetUpgradeStatus.Completed))
        {
            return new(EnumUpgradePhase.UpgradeCompleted, netConfig);
        }
        if (libraries.All(x => x.Status == EnumDotNetUpgradeStatus.None))
        {
            return new(EnumUpgradePhase.NotStarted, netConfig);
        }
        return new(EnumUpgradePhase.InProgress, netConfig);
    }
    public async Task<BasicList<LibraryNetUpdateModel>> GetLibrariesAsync(bool testing)
    {
        if (netVersionUpdateContext.IsLibraryDataPresent() == false)
        {
            return await netVersionUpdateContext.ReprocessLibrariesForUpdateAsync();
        }
        return await netVersionUpdateContext.GetLibrariesForUpdateAsync();
    }
}
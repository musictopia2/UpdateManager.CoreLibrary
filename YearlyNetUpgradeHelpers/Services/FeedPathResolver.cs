namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class FeedPathResolver(IPostBuildCommandStrategy postBuildCommand) : IFeedPathResolver
{
    string IFeedPathResolver.GetFeedPath(LibraryNetUpdateModel upgradeModel, DotNetVersionUpgradeModel netModel)
    {
        // Handle Test Mode
        if (netModel.IsTestMode)
        {
            return upgradeModel.PackageType == EnumFeedType.Local
                ? netModel.TestLocalFeedPath
                : netModel.TestPublicFeedPath;
        }
        if (postBuildCommand.ShouldRunPostBuildCommand(upgradeModel))
        {
            return ""; //because the post program will handle this  i think if this is development, the program is going to know about it anyways.  let it handle this.
        }
        // Handle Development
        if (upgradeModel.Development)
        {
            return bb1.Configuration!.GetDevelopmentPackagePath();
        }
        // Handle Local or Staging Feeds
        return upgradeModel.PackageType == EnumFeedType.Local
            ? bb1.Configuration!.GetPrivatePackagePath()
            : bb1.Configuration!.GetStagingPackagePath();
    }
}
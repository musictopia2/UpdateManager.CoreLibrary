namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class NoPostBuildCommandStrategy : IPostBuildCommandStrategy
{
    bool IPostBuildCommandStrategy.ShouldRunPostBuildCommand(LibraryNetUpdateModel libraryModel)
    {
        return false;
    }
}
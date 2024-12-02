namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IPostBuildCommandStrategy
{
    bool ShouldRunPostBuildCommand(LibraryNetUpdateModel libraryModel);
}
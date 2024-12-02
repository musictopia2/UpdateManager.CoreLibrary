namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IFeedPathResolver
{
    string GetFeedPath(LibraryNetUpdateModel upgradeModel, DotNetVersionUpgradeModel netModel);
}
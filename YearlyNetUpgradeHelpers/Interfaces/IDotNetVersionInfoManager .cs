namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IDotNetVersionInfoManager
{
    Task UpdateVersionAsync(DotNetVersionUpgradeModel model);
}
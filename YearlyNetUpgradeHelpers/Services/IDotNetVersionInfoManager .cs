namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface IDotNetVersionInfoManager
{
    Task UpdateVersionAsync(DotNetVersionUpgradeModel model);
}
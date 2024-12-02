namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IDotNetVersionInfoRepository
{
    Task<DotNetVersionUpgradeModel> GetVersionInfoAsync();
    Task SaveVersionInfoAsync(DotNetVersionUpgradeModel model);
}
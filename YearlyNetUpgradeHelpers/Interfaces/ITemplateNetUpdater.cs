namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface ITemplateNetUpdater
{
    Task<bool> UpgradeTemplateAsync<T>(TemplateModel template, BasicList<T> libraries)
        where T : class, IPackageVersionable;
}
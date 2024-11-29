namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface ITemplateNetUpdater
{
    Task<bool> UpgradeTemplateAsync<T>(TemplateModel template, BasicList<T> libraries)
        where T: class, IPackageVersionable;
}
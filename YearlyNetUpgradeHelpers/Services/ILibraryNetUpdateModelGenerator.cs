namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface ILibraryNetUpdateModelGenerator
{
    Task<BasicList<LibraryNetUpdateModel>> CreateLibraryNetUpdateModelListAsync();
}
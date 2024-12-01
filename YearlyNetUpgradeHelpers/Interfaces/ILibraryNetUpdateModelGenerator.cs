namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface ILibraryNetUpdateModelGenerator
{
    Task<BasicList<LibraryNetUpdateModel>> CreateLibraryNetUpdateModelListAsync();
}
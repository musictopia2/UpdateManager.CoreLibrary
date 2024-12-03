namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface INetVersionUpdateContext
{
    Task<BasicList<LibraryNetUpdateModel>> GetLibrariesForUpdateAsync();
    Task SaveUpdatedLibrariesAsync(BasicList<LibraryNetUpdateModel> list);
    Task ResetLibraryAsync(); //this means you need to delete the library since it has to be repopulated
    bool IsLibraryDataPresent();
    Task<BasicList<LibraryNetUpdateModel>> ReprocessLibrariesForUpdateAsync(); //if you have to update the .net then needs to redo the libraries
}
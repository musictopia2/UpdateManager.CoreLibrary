namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class FileNetVersionUpdateContext(ILibraryNetUpdateModelGenerator generator) : INetVersionUpdateContext
{
    private static readonly string _libraryPath = bb1.Configuration!.GetLibraryPath();
    async Task<BasicList<LibraryNetUpdateModel>> INetVersionUpdateContext.GetLibrariesForUpdateAsync()
    {
        BasicList<LibraryNetUpdateModel> list = await jj1.RetrieveSavedObjectAsync<BasicList<LibraryNetUpdateModel>>(_libraryPath);
        return list;
    }
    bool INetVersionUpdateContext.IsLibraryDataPresent()
    {
        return ff1.FileExists(_libraryPath);
    }
    async Task<BasicList<LibraryNetUpdateModel>> INetVersionUpdateContext.ReprocessLibrariesForUpdateAsync()
    {
        BasicList<LibraryNetUpdateModel> output = await generator.CreateLibraryNetUpdateModelListAsync();
        await jj1.SaveObjectAsync(_libraryPath, output);
        return output;
    }
    async Task INetVersionUpdateContext.ResetLibraryAsync()
    {
        await ff1.DeleteFileAsync(_libraryPath);
    }
    async Task INetVersionUpdateContext.SaveUpdatedLibrariesAsync(BasicList<LibraryNetUpdateModel> list)
    {
        await jj1.SaveObjectAsync(_libraryPath, list);
    }
}
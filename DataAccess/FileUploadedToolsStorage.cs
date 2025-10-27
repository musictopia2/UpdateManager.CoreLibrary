namespace UpdateManager.CoreLibrary.DataAccess;
public class FileUploadedToolsStorage : IUploadedToolsStorage
{
    private BasicList<UploadToolModel> _list = [];
    private readonly string _uploadedPath = bb1.Configuration!.GetUploadedToolsStoragePathFromConfig();
    private async Task SaveAsync()
    {
        try
        {
            await jj1.SaveObjectAsync(_uploadedPath, _list);
        }
        catch (Exception ex)
        {
            // Add specific file I/O exception handling here if needed
            Console.WriteLine($"Error saving the uploaded tools: {ex.Message}");
            throw;
        }
    }
    async Task IUploadedToolsStorage.AddUploadedToolAsync(UploadToolModel tool)
    {
        _list.Add(tool);
        await SaveAsync();
    }

    async Task IUploadedToolsStorage.DeleteUploadedToolAsync(string packageId)
    {
        var packageToRemove = _list.SingleOrDefault(x => x.PackageId == packageId);
        if (packageToRemove != null)
        {
            _list.RemoveSpecificItem(packageToRemove);
            await SaveAsync();
        }
        else
        {
            throw new InvalidOperationException($"Tool with ID '{packageId}' not found.");
        }
    }
    async Task<BasicList<UploadToolModel>> IUploadedToolsStorage.GetAllUploadedToolsAsync()
    {
        try
        {
            if (ff1.FileExists(_uploadedPath) == false)
            {
                _list.Clear();
                return _list.ToBasicList();
            }
            _list = await jj1.RetrieveSavedObjectAsync<BasicList<UploadToolModel>>(_uploadedPath);
            return _list.ToBasicList();
        }
        catch (Exception ex)
        {
            // Log or handle specific errors if the retrieval fails
            Console.WriteLine($"Error retrieving uploaded tools: {ex.Message}");
            throw;
        }
    }
    async Task IUploadedToolsStorage.SaveUpdatedUploadedListAsync(BasicList<UploadToolModel> list)
    {
        _list = list; //in this case, could use this list for future.
        await SaveAsync();
    }
    async Task IUploadedToolsStorage.UpdateUploadedToolAsync(UploadToolModel tool)
    {
        var found = _list.SingleOrDefault(x => x.PackageId == tool.PackageId)
           ?? throw new InvalidOperationException($"Tool '{tool.PackageId}' not found.");
        // Update the tool details
        found.NugetFilePath = tool.NugetFilePath;
        found.PackageId = tool.PackageId;
        found.Uploaded = tool.Uploaded;
        found.Version = tool.Version;
        await SaveAsync();
    }
}
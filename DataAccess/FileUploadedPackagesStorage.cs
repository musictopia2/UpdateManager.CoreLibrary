namespace UpdateManager.CoreLibrary.DataAccess;
public class FileUploadedPackagesStorage : IUploadedPackagesStorage
{
    private BasicList<UploadedPackageModel> _list = [];
    private readonly string _uploadedPath = bb1.Configuration!.GetUploadedPackagesStoragePathFromConfig();
    async Task IUploadedPackagesStorage.AddUploadedPackageAsync(UploadedPackageModel package)
    {
        _list.Add(package);
        await SaveAsync();
    }
    async Task IUploadedPackagesStorage.DeleteUploadedPackageAsync(string packageId)
    {
        var packageToRemove = _list.SingleOrDefault(x => x.PackageId == packageId);
        if (packageToRemove != null)
        {
            _list.RemoveSpecificItem(packageToRemove);
            await SaveAsync();
        }
        else
        {
            throw new InvalidOperationException($"Package with ID '{packageId}' not found.");
        }
    }
    private async Task SaveAsync()
    {
        try
        {
            await jj1.SaveObjectAsync(_uploadedPath, _list);
        }
        catch (Exception ex)
        {
            // Add specific file I/O exception handling here if needed
            Console.WriteLine($"Error saving the uploaded packages: {ex.Message}");
            throw;
        }
    }
    async Task<BasicList<UploadedPackageModel>> IUploadedPackagesStorage.GetAllUploadedPackagesAsync()
    {
        try
        {
            if (ff1.FileExists(_uploadedPath) == false)
            {
                _list.Clear();
                return _list;
            }
            _list = await jj1.RetrieveSavedObjectAsync<BasicList<UploadedPackageModel>>(_uploadedPath);
            return _list;
        }
        catch (Exception ex)
        {
            // Log or handle specific errors if the retrieval fails
            Console.WriteLine($"Error retrieving uploaded packages: {ex.Message}");
            throw;
        }
    }
    async Task IUploadedPackagesStorage.UpdateUploadedPackageAsync(UploadedPackageModel package)
    {
        var found = _list.SingleOrDefault(x => x.PackageId == package.PackageId)
           ?? throw new InvalidOperationException($"Package '{package.PackageId}' not found.");
        // Update the package details
        found.NugetFilePath = package.NugetFilePath;
        found.PackageId = package.PackageId;
        found.Uploaded = package.Uploaded;
        found.Version = package.Version;
        await SaveAsync();
    }
}
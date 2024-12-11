namespace UpdateManager.CoreLibrary.Interfaces;
public interface IUploadedPackagesStorage
{
    // Retrieves a list of all uploaded packages.
    Task<BasicList<UploadedPackageModel>> GetAllUploadedPackagesAsync();
    // Deletes an uploaded package by its ID.
    Task DeleteUploadedPackageAsync(string packageId);
    // Updates the details of an existing uploaded package.
    Task UpdateUploadedPackageAsync(UploadedPackageModel package);
    // Adds a new uploaded package to the storage system.
    Task AddUploadedPackageAsync(UploadedPackageModel package);
}
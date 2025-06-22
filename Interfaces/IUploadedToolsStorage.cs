namespace UpdateManager.CoreLibrary.Interfaces;
public interface IUploadedToolsStorage
{
    // Retrieves a list of all uploaded tools.
    Task<BasicList<UploadToolModel>> GetAllUploadedToolsAsync();
    // Deletes an uploaded tool by its ID.
    Task DeleteUploadedToolAsync(string packageId);
    // Updates the details of an existing uploaded package.
    Task UpdateUploadedToolAsync(UploadToolModel tool);
    // Adds a new uploaded tool to the storage system.
    Task AddUploadedToolAsync(UploadToolModel tool);
    Task SaveUpdatedUploadedListAsync(BasicList<UploadToolModel> list);
}
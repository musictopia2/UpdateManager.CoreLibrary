namespace UpdateManager.CoreLibrary.Interfaces;
public interface IUploadedTemplatesStorage
{
    // Retrieves a list of all uploaded tools.
    Task<BasicList<UploadTemplateModel>> GetAllUploadedTemplatesAsync();
    // Deletes an uploaded tool by its ID.
    Task DeleteUploadedTemplateAsync(string packageId);
    // Updates the details of an existing uploaded package.
    Task UpdateUploadedTemplateAsync(UploadTemplateModel template);
    // Adds a new uploaded tool to the storage system.
    Task AddUploadedTemplateAsync(UploadTemplateModel template);
    Task SaveUpdatedUploadedListAsync(BasicList<UploadTemplateModel> list);
}
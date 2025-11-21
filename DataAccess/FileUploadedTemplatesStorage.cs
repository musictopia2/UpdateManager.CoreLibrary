namespace UpdateManager.CoreLibrary.DataAccess;
public class FileUploadedTemplatesStorage : IUploadedTemplatesStorage
{
    private BasicList<UploadTemplateModel> _list = [];
    private readonly string _uploadedPath = bb1.Configuration!.UploadedTemplatesStoragePathFromConfig;
    private async Task SaveAsync()
    {
        try
        {
            await jj1.SaveObjectAsync(_uploadedPath, _list);
        }
        catch (Exception ex)
        {
            // Add specific file I/O exception handling here if needed
            Console.WriteLine($"Error saving the uploaded templates: {ex.Message}");
            throw;
        }
    }
    async Task IUploadedTemplatesStorage.AddUploadedTemplateAsync(UploadTemplateModel template)
    {
        _list.Add(template);
        await SaveAsync();
    }

    async Task IUploadedTemplatesStorage.DeleteUploadedTemplateAsync(string packageId)
    {
        var packageToRemove = _list.SingleOrDefault(x => x.PackageId == packageId);
        if (packageToRemove != null)
        {
            _list.RemoveSpecificItem(packageToRemove);
            await SaveAsync();
        }
        else
        {
            throw new InvalidOperationException($"Template with ID '{packageId}' not found.");
        }
    }

    async Task<BasicList<UploadTemplateModel>> IUploadedTemplatesStorage.GetAllUploadedTemplatesAsync()
    {
        try
        {
            if (ff1.FileExists(_uploadedPath) == false)
            {
                _list.Clear();
                return _list.ToBasicList();
            }
            _list = await jj1.RetrieveSavedObjectAsync<BasicList<UploadTemplateModel>>(_uploadedPath);
            return _list.ToBasicList();
        }
        catch (Exception ex)
        {
            // Log or handle specific errors if the retrieval fails
            Console.WriteLine($"Error retrieving uploaded templates: {ex.Message}");
            throw;
        }
    }

    async Task IUploadedTemplatesStorage.SaveUpdatedUploadedListAsync(BasicList<UploadTemplateModel> list)
    {
        _list = list; //in this case, could use this list for future.
        await SaveAsync();
    }

    async Task IUploadedTemplatesStorage.UpdateUploadedTemplateAsync(UploadTemplateModel template)
    {
        var found = _list.SingleOrDefault(x => x.PackageId == template.PackageId)
           ?? throw new InvalidOperationException($"Template '{template.PackageId}' not found.");
        // Update the tool details
        found.NugetFilePath = template.NugetFilePath;
        found.PackageId = template.PackageId;
        found.Uploaded = template.Uploaded;
        found.Version = template.Version;
        await SaveAsync();
    }
}
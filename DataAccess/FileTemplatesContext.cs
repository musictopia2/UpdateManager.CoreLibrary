namespace UpdateManager.CoreLibrary.DataAccess;
public class FileTemplatesContext : ITemplatesContext
{
    readonly string _packagePath = bb1.Configuration!.RequiredNuGetTemplatePath;
    private BasicList<NuGetTemplateModel> _list = [];
    async Task ITemplatesContext.AddTemplateAsync(NuGetTemplateModel template)
    {
        _list.Add(template);
        await jj1.SaveObjectAsync(_packagePath, _list);
    }

    async Task<BasicList<NuGetTemplateModel>> ITemplatesContext.GetTemplatesAsync()
    {
        _list.Clear();
        if (ff1.FileExists(_packagePath) == false)
        {
            return _list; //don't create it until you actually have something to save.
        }
        _list = await jj1.RetrieveSavedObjectAsync<BasicList<NuGetTemplateModel>>(_packagePath);
        return _list;
    }
    async Task ITemplatesContext.SaveCompleteListAsync(BasicList<NuGetTemplateModel> templates)
    {
        await jj1.SaveObjectAsync(_packagePath, templates);
    }
    async Task ITemplatesContext.UpdateTemplateAsync(NuGetTemplateModel updatedTemplate)
    {
        var template = _list.SingleOrDefault(x => x.PackageName == updatedTemplate.PackageName)
            ?? throw new InvalidOperationException($"Template '{updatedTemplate.PackageName}' not found.");
        // Update the package properties with the new values (only if they differ from current)
        template.Version = updatedTemplate.Version;
        template.NugetPackagePath = updatedTemplate.NugetPackagePath;
        template.IsExcluded = updatedTemplate.IsExcluded;
        // Save the updated list back to the file
        await jj1.SaveObjectAsync(_packagePath, _list);
    }

    Task ITemplatesContext.UpdateTemplateStampAsync(string templateName)
    {
        var package = _list.SingleOrDefault(x => x.PackageName == templateName)
            ?? throw new InvalidOperationException($"Template '{templateName}' not found.");
        package.LastUpdated = DateTime.Now; // Update the timestamp.
        return jj1.SaveObjectAsync(_packagePath, _list); // Save the updated list.
    }

    async Task ITemplatesContext.UpdateTemplateVersionAsync(string templateName, string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));
        }
        var package = _list.SingleOrDefault(x => x.PackageName == templateName)
            ?? throw new InvalidOperationException($"Template '{templateName}' not found.");
        package.Version = version; // Update the version.
        await jj1.SaveObjectAsync(_packagePath, _list); // Save the updated list.
    }
}
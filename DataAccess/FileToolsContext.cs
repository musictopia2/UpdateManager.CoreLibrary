namespace UpdateManager.CoreLibrary.DataAccess;
public class FileToolsContext : IToolsContext
{
    readonly string _packagePath = bb1.Configuration!.RequiredNuGetToolPath;
    private BasicList<NuGetToolModel> _list = [];
    async Task IToolsContext.AddToolAsync(NuGetToolModel tool)
    {
        _list.Add(tool);
        await jj1.SaveObjectAsync(_packagePath, _list);
    }
    async Task<BasicList<NuGetToolModel>> IToolsContext.GetToolsAsync()
    {
        _list.Clear();
        if (ff1.FileExists(_packagePath) == false)
        {
            return _list; //don't create it until you actually have something to save.
        }
        _list = await jj1.RetrieveSavedObjectAsync<BasicList<NuGetToolModel>>(_packagePath);
        return _list;
    }
    async Task IToolsContext.SaveCompleteListAsync(BasicList<NuGetToolModel> packages)
    {
        await jj1.SaveObjectAsync(_packagePath, packages);
    }
    async Task IToolsContext.UpdateToolAsync(NuGetToolModel updatedTool)
    {
        var tool = _list.SingleOrDefault(x => x.PackageName == updatedTool.PackageName)
            ?? throw new InvalidOperationException($"Tool '{updatedTool.PackageName}' not found.");
        // Update the package properties with the new values (only if they differ from current)
        tool.Version = updatedTool.Version;
        tool.NugetPackagePath = updatedTool.NugetPackagePath;
        tool.IsExcluded = updatedTool.IsExcluded;
        // Save the updated list back to the file
        await jj1.SaveObjectAsync(_packagePath, _list);
    }
    async Task IToolsContext.UpdateToolVersionAsync(string toolName, string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));
        }
        var package = _list.SingleOrDefault(x => x.PackageName == toolName)
            ?? throw new InvalidOperationException($"Tool '{toolName}' not found.");
        package.Version = version; // Update the version.
        await jj1.SaveObjectAsync(_packagePath, _list); // Save the updated list.
    }
}
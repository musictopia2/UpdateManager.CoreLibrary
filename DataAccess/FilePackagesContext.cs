namespace UpdateManager.CoreLibrary.DataAccess;
public class FilePackagesContext : IPackagesContext
{
    public static Func<string, Task>? AfterSaveAsync { get; set; }
    readonly string _packagePath = bb1.Configuration!.RequiredNuGetPackagesPath;
    private BasicList<NuGetPackageModel> _list = [];
    async Task IPackagesContext.AddPackageAsync(NuGetPackageModel package)
    {
        _list.Add(package);
        await jj1.SaveObjectAsync(_packagePath, _list);
    }
    async Task<BasicList<NuGetPackageModel>> IPackagesContext.GetPackagesAsync()
    {
        _list.Clear();
        if (ff1.FileExists(_packagePath) == false)
        {
            return _list; //don't create it until you actually have something to save.
        }
        _list = await jj1.RetrieveSavedObjectAsync<BasicList<NuGetPackageModel>>(_packagePath);
        return _list;
    }
    async Task IPackagesContext.SaveCompleteListAsync(BasicList<NuGetPackageModel> packages)
    {
        _list = packages;
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        await jj1.SaveObjectAsync(_packagePath, _list);
        if (AfterSaveAsync is { } afterSave)
        {
            await afterSave(_packagePath);
        }
    }


    async Task IPackagesContext.UpdatePackageAsync(NuGetPackageModel updatedPackage)
    {
        var package = _list.SingleOrDefault(x => x.PackageName == updatedPackage.PackageName)
            ?? throw new InvalidOperationException($"Package '{updatedPackage.PackageName}' not found.");
        // Update the package properties with the new values (only if they differ from current)
        package.Version = updatedPackage.Version;
        package.Framework = updatedPackage.Framework;
        package.NugetPackagePath = updatedPackage.NugetPackagePath;
        package.IsExcluded = updatedPackage.IsExcluded;
        package.RequiredBy = updatedPackage.RequiredBy;
        await SaveAsync();
    }

    async Task IPackagesContext.UpdatePackageVersionAsync(string packageName, string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));
        }
        var package = _list.SingleOrDefault(x => x.PackageName == packageName)
            ?? throw new InvalidOperationException($"Package '{packageName}' not found.");
        package.Version = version; // Update the version.
        await SaveAsync();
    }
}
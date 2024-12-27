namespace UpdateManager.CoreLibrary.DataAccess;
public class FilePackagesContext : IPackagesContext
{
    readonly string _packagePath = bb1.Configuration!.GetRequiredNuGetPackagesPath();
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
        await jj1.SaveObjectAsync(_packagePath, packages);
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

        // Save the updated list back to the file
        await jj1.SaveObjectAsync(_packagePath, _list);
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
        await jj1.SaveObjectAsync(_packagePath, _list); // Save the updated list.
    }
}
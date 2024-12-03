namespace UpdateManager.CoreLibrary.Interfaces;
public interface IPackagesContext
{
    // Retrieves the list of all NuGet packages.
    Task<BasicList<NuGetPackageModel>> GetPackagesAsync();

    // Updates only the version of the package identified by its name.
    Task UpdatePackageVersionAsync(string packageName, string version);

    // Updates the properties of a package with the provided model.
    Task UpdatePackageAsync(NuGetPackageModel package);

    // Adds a new package to the list.
    Task AddPackageAsync(NuGetPackageModel package);
}
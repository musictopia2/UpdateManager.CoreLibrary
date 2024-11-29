namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class LibraryNetUpdateModelGenerator(IPackagesContext packageContext) : ILibraryNetUpdateModelGenerator
{
    async Task<BasicList<LibraryNetUpdateModel>> ILibraryNetUpdateModelGenerator.CreateLibraryNetUpdateModelListAsync()
    {
        BasicList<NuGetPackageModel> packages = await packageContext.GetPackagesAsync();
        BasicList<LibraryNetUpdateModel> output = [];
        // Check if we need to filter packages for testing
        bool includeOnlyTests = LibraryInclusionGlobals.LibrariesToIncludeForTest.Count > 0;
        foreach (var package in packages)
        {
            if (package.TemporarilyIgnore)
            {
                continue; //ignore those.
            }
            if (includeOnlyTests && !LibraryInclusionGlobals.LibrariesToIncludeForTest
                .Any(testLibrary => string.Equals(testLibrary, package.PackageName, StringComparison.OrdinalIgnoreCase)))
            {
                continue; // Skip this package as it's not in the test list
            }
            LibraryNetUpdateModel upgrade = new ()
            {
                PackageType = package.FeedType,
                PackageName = package.PackageName,
                CsProjPath = package.CsProjPath,
                NugetPackagePath = package.NugetPackagePath,
                Status = EnumDotNetUpgradeStatus.None
            };
            upgrade.Version = package.FeedType == EnumFeedType.Local
                ? package.Version.IncrementMinorVersion()
                : "0.0.1"; // Placeholder version for non-local packages, should be updated later
            output.Add(upgrade);
        }
        output = LibraryDependencyResolver.ResolveDependencies(output); //needs to resolve dependencies as part of the steps.
        return output;
    }
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class LibraryDotNetUpgraderBuild : ILibraryDotNetUpgraderBuild
{
    async Task<bool> ILibraryDotNetUpgraderBuild.BuildLibraryAsync(LibraryNetUpdateModel libraryModel, DotNetVersionUpgradeModel dotNetModel, BasicList<LibraryNetUpdateModel> libraries, CancellationToken cancellationToken)
    {
        CsProjEditor editor = new(libraryModel.CsProjPath);
        string netVersion = bb1.Configuration!.GetNetVersion();
        bool rets;
        rets = editor.UpdateNetVersion(netVersion);
        if (rets == false)
        {
            return false;
        }
        rets = await editor.UpdateDependenciesAsync(libraries);
        if (rets == false)
        {
            return false;
        }
        rets = await ProjectBuilder.BuildProjectAsync(libraryModel.CsProjPath, "/p:SkipPostBuild=true", cancellationToken);
        return rets;
    }
    async Task<bool> ILibraryDotNetUpgraderBuild.AlreadyUpgradedAsync(LibraryNetUpdateModel upgradeModel, DotNetVersionUpgradeModel dotNetModel)
    {
        if (dotNetModel.IsTestMode)
        {
            return false; //since its testing, go ahead and go through the process of upgrading no matter what since its only testing anyways
        }
        string netVersion = bb1.Configuration!.GetNetVersion();
        if (upgradeModel.PackageType == EnumFeedType.Public)
        {
            //has to check something like 9.0.1.  if there on public nuget, then already done period.
            string upgradeVersion = $"{netVersion}.0.1";
            bool rets;
            rets = await NuGetPackageChecker.IsPackageAvailableAsync(upgradeModel.PackageName, upgradeVersion);
            return rets;
        }

        string lastVersion = DecrementVersion(upgradeModel.Version);
        string feedPath = bb1.Configuration!.GetPrivatePackagePath();
        string packagePath = Path.Combine(feedPath, upgradeModel.PackageName, lastVersion);
        string packageNetVersion = GetNetVersionFromNuspec(upgradeModel.PackageName, packagePath);
        return packageNetVersion == netVersion;
    }
    // Method to get the .NET version (e.g., "9" from "net9.0") from the .nuspec file
    private static string GetNetVersionFromNuspec(string packageName, string packagePath)
    {
        string nuspecPath = Path.Combine(packagePath, $"{packageName.ToLower()}.nuspec");

        if (File.Exists(nuspecPath))
        {
            XDocument nuspecXml = XDocument.Load(nuspecPath);

            // Find the targetFramework attribute under the dependencies group
            var targetFrameworkElement = nuspecXml.Descendants()
                                                   .Where(e => e.Name.LocalName == "group" && e.Attribute("targetFramework") != null)
                                                   .FirstOrDefault();
            if (targetFrameworkElement != null)
            {
                // Extract the version from "net9.0" -> "9"
                var targetFramework = targetFrameworkElement.Attribute("targetFramework")?.Value;
                if (targetFramework?.StartsWith("net") == true)
                {
                    return targetFramework.Substring(3); // returns "9" from "net9.0"
                }
                throw new CustomBasicException($"The .nuspec file at {nuspecPath} does not contain a valid target framework version. Please ensure the file is structured correctly.");
            }
            else
            {
                throw new CustomBasicException($"The .nuspec file at {nuspecPath} does not contain a valid target framework version. Please ensure the file is structured correctly.");
            }
        }
        else
        {
            throw new CustomBasicException($"The path {nuspecPath} does not exist");
        }
    }
    // Method to decrement a version string (e.g., 1.0.86 -> 1.0.85)
    private static string DecrementVersion(string version)
    {
        var versionParts = version.Split('.');

        if (versionParts.Length != 3)
        {
            throw new ArgumentException("Invalid version format");
        }

        int patchVersion = int.Parse(versionParts[2]);
        patchVersion--; // Decrement the patch version

        // If patch version is negative, handle accordingly (e.g., reset to 0)
        if (patchVersion <= 0)
        {
            throw new CustomBasicException($"Invalid version decrement: The patch version cannot go below 1.0.1. Current version: {version}. This indicates a misconfiguration or an unexpected versioning error.");
        }
        // Rebuild the version string
        return $"{versionParts[0]}.{versionParts[1]}.{patchVersion}";
    }
}
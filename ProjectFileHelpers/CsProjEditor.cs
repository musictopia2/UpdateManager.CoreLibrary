﻿namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
public class CsProjEditor(string csprojPath)
{
    private readonly XDocument _csProjDoc = XDocument.Load(csprojPath);
    private bool _anyUpdate = false;  // This is now private
    private XElement? _root;
    private bool CanGetRoot()
    {
        _root = _csProjDoc.Root;
        return _root != null;
    }
    public bool AddFeedType(EnumFeedType feedType)
    {
        // Check if we can get the root element of the XML
        if (CanGetRoot() == false)
        {
            return false;
        }

        // Add or update FeedType inside the PropertyGroup
        XElement? propertyGroup = _root!.Descendants("PropertyGroup").FirstOrDefault();
        if (propertyGroup == null)
        {
            propertyGroup = new XElement("PropertyGroup");
            _root.Add(propertyGroup);  // Add a PropertyGroup if it doesn't exist
        }

        // Define the FeedType value (either "Private" or "Public")
        string feedTypeValue = feedType == EnumFeedType.Local ? "Local" : "Public";

        // Look for existing FeedType element
        var feedTypeElement = propertyGroup.Descendants("FeedType").FirstOrDefault();

        if (feedTypeElement != null)
        {
            // If the FeedType element already exists, update its value
            feedTypeElement.Value = feedTypeValue;
        }
        else
        {
            // If no FeedType element exists, add one
            propertyGroup.Add(new XElement("FeedType", feedTypeValue));
        }

        // Mark that the file has been modified
        _anyUpdate = true;
        return true;
    }
    // Method to get the current FeedType (Local or Public) from the .csproj
    public EnumFeedType? GetFeedType()
    {
        if (!CanGetRoot())
        {
            return null;
        }

        // Find the FeedType element in the .csproj file
        var feedTypeElement = _root!.Descendants("PropertyGroup")
                                    .Descendants("FeedType")
                                    .FirstOrDefault();
        if (feedTypeElement == null)
        {
            return null;
        }

        // Parse the FeedType value and return the corresponding Enum value
        string feedTypeValue = feedTypeElement.Value.Trim();

        // Convert the FeedType value to EnumFeedType
        return feedTypeValue switch
        {
            "Local" => EnumFeedType.Local,
            "Public" => EnumFeedType.Public,
            _ => null // Return null if the value is invalid
        };
    }

    public bool UpdateNetVersion(string newNetVersion)
    {
        if (CanGetRoot() == false)
        {
            return false;
        }
        //net9.0-windows
        // Check if the application is a Windows app by looking for OutputType=WinExe
        var outputTypeElement = _root!.Descendants("OutputType").FirstOrDefault();
        if (outputTypeElement != null && outputTypeElement.Value.Equals("WinExe", StringComparison.OrdinalIgnoreCase))
        {
            // If it's a Windows app (WinExe), update the version in the format: Windows[version]net
            var targetFrameworkElement = _root.Descendants("TargetFramework").FirstOrDefault();
            if (targetFrameworkElement != null)
            {
                string originalValue = targetFrameworkElement.Value;
                string newValue = $"net{newNetVersion}.0-windows";  // For Windows apps, use the new format
                                                                   // Update the version if it is different
                if (!originalValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
                {
                    targetFrameworkElement.Value = newValue;
                    _anyUpdate = true;
                }
                return true;
            }
        }
        else
        {
            // Otherwise, update the version normally
            var targetFrameworkElement = _root!.Descendants("TargetFramework").FirstOrDefault() ??
                                         _root.Descendants("TargetFrameworks").FirstOrDefault(); // Handle multiple target frameworks
            if (targetFrameworkElement != null)
            {
                string originalValue = targetFrameworkElement.Value;
                string newValue = $"net{newNetVersion}.0";
                // Update the version if it is different
                if (!originalValue.Equals(newValue, StringComparison.OrdinalIgnoreCase))
                {
                    targetFrameworkElement.Value = newValue;
                    _anyUpdate = true;
                }
                return true;
            }
        }
        return false;
    }


    // Generic method to update dependencies (NuGet or .NET version)
    public async Task<bool> UpdateDependenciesAsync<T>(BasicList<T> libraries) where T : IPackageVersionable
    {
        if (CanGetRoot() == false)
        {
            return false;
        }
        // Update <PackageReference> elements
        var packageReferences = _root!.Descendants("PackageReference").ToList();
        // Loop through each package in the dependencies
        foreach (var packageReference in packageReferences)
        {
            string packageName = packageReference.Attribute("Include")?.Value!;
            string currentVersion = packageReference.Attribute("Version")?.Value!;
            if (string.IsNullOrEmpty(currentVersion))
            {
                Console.WriteLine($"[Warning] Package {packageName} does not have a version specified. Skipping.");
                return false; //i think that all dependencies need a version.
            }

            //try this part.

            //if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(currentVersion))
            //{
            //    continue;
            //}

            // Skip if PrivateAssets="all"
            if (ExcludedDependencies.ExcludedPackagesAll.Contains(packageName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }
            //has to now try to load in private assets.  because otherwise, some web assembly apps can't be updated otherwise.
            //if (packageReference.Attribute("PrivateAssets")?.Value == "all")
            //{
            //    continue;
            //}

            // Lookup for the custom version from the provided libraries list
            var customLibrary = libraries.FirstOrDefault(lib => string.Equals(lib.PackageName, packageName, StringComparison.OrdinalIgnoreCase));
            if (customLibrary != null)
            {
                // If custom version exists in the LibraryConfig list, use it
                //Console.WriteLine($"Using custom version {customLibrary.PackageVersion} for {packageName}");
                if (customLibrary.Version == "")
                {
                    return false; //because the package version is not supposed to be blank.
                }
                packageReference.SetAttributeValue("Version", customLibrary.Version);
                _anyUpdate = true;
                continue; // Skip the public version check for custom packages
            }

            // If package not found in the custom feed, check for the latest version on NuGet public
            bool isExcluded = ExcludedDependencies.ExcludedExceptMajor.Contains(packageName, StringComparer.OrdinalIgnoreCase);
            string latestVersion = await NuGetPackageChecker.GetLatestPublicVersionAsync(packageName);
            if (string.IsNullOrEmpty(latestVersion))
            {
                Console.WriteLine($"Package name {packageName} was not found on nuget and was not on my custom list");
                return false; // If there is no public version and it's not custom, we can't update
            }
            if (isExcluded)
            {
                latestVersion = AdjustToZeroPatchVersion(latestVersion);
                if (await NuGetPackageChecker.IsPublicPackageAvailableAsync(packageName, latestVersion) == false)
                {
                    //this means this is not available.  just skip for now.
                    continue;
                }
            }

            // Compare and update if necessary
            if (string.Compare(currentVersion, latestVersion) < 0)
            {
                //Console.WriteLine($"Updating {packageName} from {currentVersion} to {latestVersion}");
                packageReference.SetAttributeValue("Version", latestVersion);
                _anyUpdate = true;
            }
        }
        return true;
    }
    private static string AdjustToZeroPatchVersion(string latestVersion)
    {
        var versionParts = latestVersion.Split('.');
        // If the version has at least 3 parts, we modify the patch part to 0
        if (versionParts.Length >= 3)
        {
            versionParts[2] = "0"; // Set the patch version to 0
        }
        // Rebuild the version string by joining the first two parts (major.minor) and the patch part (0)
        return string.Join(".", versionParts.Take(3));
    }
    public bool RemovePostBuildCommand()
    {
        // Check if the root of the XML document exists
        if (!CanGetRoot())
        {
            return false;
        }

        // Find and remove the PostBuild target (if it exists)
        var postBuildTarget = _root!.Descendants("Target")
            .FirstOrDefault(t => t.Attribute("Name")?.Value == "PostBuild");

        if (postBuildTarget != null)
        {
            postBuildTarget.Remove();
            _anyUpdate = true;
        }


        // Find the first PropertyGroup element
        XElement? propertyGroup = _root.Descendants("PropertyGroup").FirstOrDefault();
        if (propertyGroup != null)
        {
            // Find and remove the RunPostBuildAppCondition property inside this PropertyGroup
            var runPostBuildCondition = propertyGroup.Descendants("RunPostBuildAppCondition").FirstOrDefault();
            if (runPostBuildCondition != null)
            {
                runPostBuildCondition.Remove();
                _anyUpdate = true;
            }

        }


        // If anything was updated, return true, indicating that changes were made
        return _anyUpdate;
    }
    public bool AddPostBuildCommand(string programDirectory, bool allowSkipBuild)
    {
        // Check if the root of the XML document exists
        if (!CanGetRoot())
        {
            return false;
        }

        // Check if the PostBuild target already exists
        var existingPostBuildTarget = _root!.Descendants("Target")
            .FirstOrDefault(t => t.Attribute("Name")?.Value == "PostBuild");

        // If a PostBuild target exists, return false (as it can only exist once)
        if (existingPostBuildTarget != null)
        {
            Console.WriteLine("[Info] PostBuild target already exists.");
            return false;
        }

        // Define the PostBuild target with the necessary command
        string netVersion = bb1.Configuration!.GetNetVersion();  // Assuming this method gives the correct .NET version
        string fullPath = Path.Combine(programDirectory, "bin", "Release", $"net{netVersion}.0");
        string programName = ff1.FileName(programDirectory); // Assuming this gets the program's filename
        string programPath = Path.Combine(fullPath, $"{programName}.exe");

        // Ensure the program file exists
        if (!ff1.FileExists(programPath))
        {
            throw new CustomBasicException($"Path to the program {programPath} not found.");
        }

        // Ensure the PropertyGroup element exists
        XElement propertyGroup = _root.Descendants("PropertyGroup").FirstOrDefault()!;
        if (propertyGroup == null)
        {
            propertyGroup = new XElement("PropertyGroup");
            _root.Add(propertyGroup);  // Add a PropertyGroup if it doesn't exist
        }

        // Add the RunPostBuildAppCondition property if SkipPostBuild is true
        if (allowSkipBuild)
        {
            // Only add this condition if it does not already exist
            var runPostBuildCondition = propertyGroup.Descendants("RunPostBuildAppCondition").FirstOrDefault();
            if (runPostBuildCondition == null)
            {
                propertyGroup.Add(new XElement("RunPostBuildAppCondition",
                    new XAttribute("Condition", "'$(Configuration)' == 'Release'"), "true"));
            }
        }

        // Create a function to wrap paths in quotes if they contain spaces
        static string WrapInQuotesIfNeeded(string argument)
        {
            return argument.Contains(' ') ? $"\"{argument}\"" : argument;
        }

        // Build the Exec command for the PostBuild process
        string execCommand = $"{WrapInQuotesIfNeeded(programPath)} " +
                             $"{WrapInQuotesIfNeeded("$(ProjectName)")} " +
                             $"{WrapInQuotesIfNeeded("$(ProjectDir)")} " +
                             $"{WrapInQuotesIfNeeded("$(ProjectFileName)")} " +
                             $"{WrapInQuotesIfNeeded("$(TargetDir)")}";


        // Create the PostBuild target element
        XElement postBuildTarget;

        // For allowSkipBuild == true, we include the condition checking for the skip flag
        if (allowSkipBuild)
        {
            postBuildTarget = new XElement("Target",
            new XAttribute("Name", "PostBuild"),
            new XAttribute("AfterTargets", "PostBuildEvent"),
            new XElement("Exec",
                new XAttribute("Command", execCommand),
                new XAttribute("Condition", "'$(Configuration)' == 'Release' and '$(RunPostBuildAppCondition)' == 'true'")));
        }
        else
        {
            postBuildTarget = new XElement("Target",
            new XAttribute("Name", "PostBuild"),
            new XAttribute("AfterTargets", "PostBuildEvent"),
            new XElement("Exec",
            new XAttribute("Command", execCommand),
            new XAttribute("Condition", "'$(Configuration)' == 'Release'")
            ));
        }

        // Add the new PostBuild target to the root element of the .csproj
        _root.Add(postBuildTarget);

        // Mark that an update was made
        _anyUpdate = true;

        return true; // Successfully added the PostBuild command
    }
    public string VersionUsed()
    {
        if (CanGetRoot() == false)
        {
            throw new CustomBasicException("Unable to get root");
        }
        var targetFrameworkElement = _root!.Descendants("TargetFramework").FirstOrDefault() ??
                                     _root.Descendants("TargetFrameworks").FirstOrDefault(); // Handle multiple target frameworks
        if (targetFrameworkElement != null)
        {
            string originalValue = targetFrameworkElement.Value;

            return ExtractMajorVersion(originalValue);
        }
        throw new CustomBasicException("Unable to get version");
    }
    private static string ExtractMajorVersion(string targetFramework)
    {
        var prefix = "net";
        if (targetFramework.StartsWith(prefix))
        {
            var versionString = targetFramework.Substring(prefix.Length);  // e.g., "8.0"
            var majorVersionString = versionString.Split('.')[0];  // Get the part before the dot
            return majorVersionString;
        }
        throw new CustomBasicException("Invalid target framework format.");
    }
    public EnumTargetFramework GetTargetFramework()
    {
        if (CanGetRoot() == false)
        {
            throw new CustomBasicException("Unable to get root");
        }
        var targetFrameworkElement = _root!.Descendants("TargetFramework").FirstOrDefault() ??
                                     _root.Descendants("TargetFrameworks").FirstOrDefault(); // Handle multiple target frameworks
        if (targetFrameworkElement != null)
        {
            string originalValue = targetFrameworkElement.Value;

            if (originalValue.Contains("netstandard2", StringComparison.CurrentCultureIgnoreCase))
            {
                return EnumTargetFramework.NetStandard;
            }
            return EnumTargetFramework.NetRuntime;
        }
        throw new CustomBasicException("Unable to get version");
    }

    // Update the PostBuild command's .NET version (if required)
    public bool UpdatePostBuildCommand(string newNetVersion)
    {
        if (CanGetRoot() == false)
        {
            return false;
        }
        var postBuildTarget = _root!.Descendants("Target")
            .FirstOrDefault(t => t.Attribute("Name")?.Value == "PostBuild");

        if (postBuildTarget == null)
        {
            Console.WriteLine("[Error] PostBuild target not found.");
            return false;
        }
        var execElement = postBuildTarget.Descendants("Exec").FirstOrDefault();
        if (execElement != null)
        {
            string originalCommand = execElement.Attribute("Command")?.Value ?? "";
            string versionPattern = @"net\d+\.\d+";  // Regex to match netX.X (e.g., net9.0)
            var versionMatch = System.Text.RegularExpressions.Regex.Match(originalCommand, versionPattern);
            if (versionMatch.Success)
            {
                string oldVersion = versionMatch.Value;
                string newVersion = $"net{newNetVersion}.0";
                string updatedCommand = originalCommand.Replace(oldVersion, newVersion);
                updatedCommand = updatedCommand.Replace("&#xD;&#xA;", "");
                execElement.SetAttributeValue("Command", updatedCommand);
                _anyUpdate = true;
                return true;
            }
        }
        return false;
    }
    public bool IsAnalzyer()
    {
        if (CanGetRoot() == false)
        {
            throw new CustomBasicException("Unable to get root");
        }
        var buildoutput = _root!.Descendants("IncludeBuildOutput").FirstOrDefault();
        if (buildoutput is null)
        {
            return false;
        }
        if (buildoutput.Value == "false")
        {
            return true;
        }
        return false;
    }

    // Saves the updated csproj file if any changes have been made
    public void SaveChanges()
    {
        if (_anyUpdate)
        {
            _csProjDoc.Save(csprojPath);
        }

    }
}
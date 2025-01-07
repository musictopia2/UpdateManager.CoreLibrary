namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
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
    public bool IsWindows()
    {
        if (CanGetRoot() == false)
        {
            throw new CustomBasicException("Cannot get root");
        }
        var wpfElement = _root!.Descendants("UseWPF").FirstOrDefault();
        if (wpfElement is null)
        {
            return false;
        }
        if (wpfElement.Value == "true")
        {
            return true;
        }
        return false;
    }
    public bool UpdateNetVersion(string newNetVersion)
    {
        if (CanGetRoot() == false)
        {
            return false;
        }
        //net9.0-windows
        // Check if the application is a Windows app by looking for OutputType=WinExe
        var wpfElement = _root!.Descendants("UseWPF").FirstOrDefault();
        if (wpfElement != null && wpfElement.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
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
                string newValue;
                newValue = $"net{newNetVersion}.0";
                if (originalValue.EndsWith("-android"))
                {
                    newValue = $"{newValue}-android";
                }
                
                
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
        // Find all ItemGroup elements
        var itemGroups = _root!.Descendants("ItemGroup").ToList();

        // Loop through each ItemGroup to find and update the PackageReference elements
        foreach (var itemGroup in itemGroups)
        {
            // Check if this ItemGroup has a Condition attribute
            var conditionAttribute = itemGroup.Attribute("Condition");
            bool isDebugConfiguration = conditionAttribute != null && conditionAttribute.Value.Contains("Debug");
            bool isReleaseConfiguration = conditionAttribute != null && conditionAttribute.Value.Contains("Release");

            // Loop through each PackageReference in the ItemGroup
            var packageReferences = itemGroup.Descendants("PackageReference").ToList();
            foreach (var packageReference in packageReferences)
            {
                string packageName = packageReference.Attribute("Include")?.Value!;
                string currentVersion = packageReference.Attribute("Version")?.Value!;
                if (currentVersion.Equals("$(mauiVersion)", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(currentVersion))
                {
                    return false; // Every dependency must have a version
                }
                // Skip if PrivateAssets="all"
                //can't skip privateassets since there is something in webassembly that needs it.

                //if (ExcludedDependencies.ExcludedPackagesAll.Contains(packageName, StringComparer.OrdinalIgnoreCase))
                //{
                //    continue;
                //}

                // Lookup for the custom version from the provided libraries list
                var customLibrary = libraries.FirstOrDefault(lib => string.Equals(lib.GetPackageID(), packageName, StringComparison.OrdinalIgnoreCase));
                if (customLibrary != null)
                {
                    // If a custom version exists, update it
                    if (string.IsNullOrEmpty(customLibrary.Version))
                    {
                        return false; // Package version cannot be blank
                    }

                    packageReference.SetAttributeValue("Version", customLibrary.Version);
                    _anyUpdate = true;
                    continue; // Skip the public version check for custom packages
                }

                // If not found in the custom list, check for the latest version on NuGet public
                bool isExcluded = ExcludedDependencies.ExcludedExceptMajor.Contains(packageName, StringComparer.OrdinalIgnoreCase);
                string latestVersion = await NuGetPackageChecker.GetLatestPublicVersionAsync(packageName);
                if (string.IsNullOrEmpty(latestVersion))
                {
                    return false; // Can't update if not found in NuGet or the custom list
                }

                if (isExcluded)
                {
                    // Adjust version to zero patch if excluded except for major
                    latestVersion = AdjustToZeroPatchVersion(latestVersion);
                    if (await NuGetPackageChecker.IsPublicPackageAvailableAsync(packageName, latestVersion) == false)
                    {
                        continue; // Skip if the adjusted version is unavailable
                    }
                }

                // Parse the versions as Version objects
                Version currentVersionObj = new (currentVersion);
                Version latestVersionObj = new (latestVersion);

                if (currentVersionObj.CompareTo(latestVersionObj) < 0)
                {
                    // Update the package reference to the latest version
                    packageReference.SetAttributeValue("Version", latestVersion);
                    _anyUpdate = true;
                }
            }
        }
        return true; //even if there was no changes, if this did not fail, then has to return true.  otherwise, i may think there are errors when there are no errors.
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
    public bool AddProjectReferencesBasedOnConfig(BasicList<PackagePathModel> packages)
    {
        if (!CanGetRoot())
        {
            Console.WriteLine("Failed to get root element. Returning false.");
            return false;
        }

        bool anyUpdate = false;

        // Iterate through the packages and update the ItemGroup references
        foreach (var package in packages)
        {
            // Retrieve the correct version for the package
            string packageVersion = GetPackageVersion(package.PackageName);

            if (string.IsNullOrEmpty(packageVersion))
            {
                throw new CustomBasicException("No Version found for package: " + package.PackageName);
            }

            // Find all ItemGroups
            var allItemGroups = _root!.Descendants("ItemGroup").ToList();

            foreach (var itemGroup in allItemGroups)
            {
                // If the ItemGroup has no Condition attribute, check for PackageReference and remove it
                if (itemGroup.Attribute("Condition") == null)
                {
                    // Find the PackageReference within this ItemGroup
                    var existingPackageReference = itemGroup.Descendants("PackageReference")
                        .FirstOrDefault(p => p.Attribute("Include")?.Value == package.PackageName);
                    if (existingPackageReference != null)
                    {
                        // Remove the PackageReference if found
                        existingPackageReference.Remove();
                        anyUpdate = true;  // Mark as updated since the reference is removed
                    }
                }
            }

            // Create a new PackageReference for Release mode
            XElement newPackageReference = new ("PackageReference",
                new XAttribute("Include", package.PackageName),
                new XAttribute("Version", packageVersion) // Ensure this returns the correct version
            );

            // Handle Debug mode (add ProjectReference)
            var debugItemGroup = _root.Descendants("ItemGroup")
                .FirstOrDefault(p => p.Attribute("Condition")?.Value == "'$(Configuration)' == 'Debug'");

            // If ItemGroup for Debug doesn't exist, create it
            if (debugItemGroup == null)
            {
                debugItemGroup = new ("ItemGroup",
                    new XAttribute("Condition", "'$(Configuration)' == 'Debug'"));
                _root.Add(debugItemGroup);
                anyUpdate = true;
            }

            // Add the ProjectReference for Debug mode
            var existingProjectReference = debugItemGroup.Descendants("ProjectReference")
                .FirstOrDefault(pr => pr.Attribute("Include")?.Value == package.PathToProject);

            if (existingProjectReference == null)
            {
                XElement projectReference = new ("ProjectReference",
                    new XAttribute("Include", package.PathToProject)
                );
                debugItemGroup.Add(projectReference);
                anyUpdate = true;
            }

            // Handle Release mode (add PackageReference)
            var releaseItemGroup = _root.Descendants("ItemGroup")
                .FirstOrDefault(p => p.Attribute("Condition")?.Value == "'$(Configuration)' == 'Release'");

            // If ItemGroup for Release doesn't exist, create it
            if (releaseItemGroup == null)
            {
                releaseItemGroup = new XElement("ItemGroup",
                    new XAttribute("Condition", "'$(Configuration)' == 'Release'"));
                _root.Add(releaseItemGroup);
                anyUpdate = true;
            }

            // Check if the PackageReference for Release already exists, otherwise add it
            var existingReleasePackageReference = releaseItemGroup.Descendants("PackageReference")
                .FirstOrDefault(pr => pr.Attribute("Include")?.Value == package.PackageName);

            if (existingReleasePackageReference == null)
            {
                releaseItemGroup.Add(newPackageReference);
                anyUpdate = true;
            }
        }

        // If any updates were made, mark the change
        if (anyUpdate)
        {
            _anyUpdate = true;
        }

        return anyUpdate;
    }

    // Method to remove all ProjectReference elements from the .csproj file
    public bool RemoveAllProjectReferences()
    {
        if (!CanGetRoot())
        {
            Console.WriteLine("Failed to get root element. Returning false.");
            return false;
        }

        bool anyUpdate = false;

        // Find all ItemGroup elements
        var allItemGroups = _root!.Descendants("ItemGroup").ToList();

        // Loop through each ItemGroup and remove ProjectReference elements
        foreach (var itemGroup in allItemGroups)
        {
            var projectReferences = itemGroup.Descendants("ProjectReference").ToList();
            foreach (var projectReference in projectReferences)
            {
                projectReference.Remove(); // Remove each ProjectReference
                anyUpdate = true;
            }
        }

        // If any ProjectReferences were removed, mark the change
        if (anyUpdate)
        {
            _anyUpdate = true;
        }

        return anyUpdate;
    }
    public bool AddPackageReference(NuGetPackageModel package)
    {
        if (!CanGetRoot())
        {
            Console.WriteLine("Failed to get root element. Returning false.");
            return false;
        }

        // Find all ItemGroup elements
        var itemGroups = _root!.Descendants("ItemGroup").ToList();

        // Create the new PackageReference element with the package information
        XElement newPackageReference = new("PackageReference",
            new XAttribute("Include", package.PackageName),
            new XAttribute("Version", package.Version)
        );

        bool updated = false;

        // Step 1: Remove any "hosed" PackageReference from any ItemGroup that already contains it.
        foreach (var itemGroup in itemGroups)
        {
            var existingPackageReference = itemGroup.Descendants("PackageReference")
                .FirstOrDefault(p => p.Attribute("Include")?.Value == package.PackageName);
            int count = itemGroup.Descendants("PackageReference").Count();
            if (existingPackageReference != null && count == 1)
            {
                // Remove the hosed reference
                existingPackageReference.Remove();
                updated = true;
            }
        }

        // Step 2: Check if the PackageReference is already present in the correct ItemGroup
        var targetItemGroup = itemGroups.FirstOrDefault(ig =>
            ig.Descendants("PackageReference")
              .Any(p => p.Attribute("Include")?.Value == package.PackageName));

        // If no ItemGroup exists with the correct reference, create a new one and add the new PackageReference
        if (targetItemGroup == null)
        {
            // Look for an existing ItemGroup that should receive the new PackageReference
            targetItemGroup = _root.Descendants("ItemGroup")
                .FirstOrDefault(ig => ig.Descendants("PackageReference").Any());

            // If no valid ItemGroup exists, create a new one
            if (targetItemGroup == null)
            {
                targetItemGroup = new XElement("ItemGroup");
                _root.Add(targetItemGroup); // Add to the root
            }
        }

        // Step 3: Add the PackageReference to the found or newly created ItemGroup
        var existingPackageReferenceInTargetGroup = targetItemGroup.Descendants("PackageReference")
            .FirstOrDefault(p => p.Attribute("Include")?.Value == package.PackageName);

        if (existingPackageReferenceInTargetGroup == null)
        {
            targetItemGroup.Add(newPackageReference);  // Add to the correct ItemGroup
            updated = true;
        }

        // Step 4: Remove any empty ItemGroup elements (those without any PackageReference or relevant elements)
        var emptyItemGroups = _root.Descendants("ItemGroup")
            .Where(ig => !ig.HasElements)  // Check if ItemGroup has no child elements
            .ToList();

        foreach (var emptyGroup in emptyItemGroups)
        {
            emptyGroup.Remove();  // Remove the empty ItemGroup
            updated = true;
        }

        // Step 5: If any update was made, mark _anyUpdate to true
        if (updated)
        {
            _anyUpdate = true;
        }

        return updated;
    }



    private string GetPackageVersion(string packageName)
    {
        // Logic to get the version for the package. If it's already in the csproj, you can fetch it from there.
        var packageReference = _root!.Descendants("PackageReference")
            .FirstOrDefault(p => p.Attribute("Include")?.Value == packageName);

        if (packageReference != null)
        {
            return packageReference.Attribute("Version")?.Value ?? string.Empty;
        }

        // If the package is not found, you can handle this based on your needs.
        return string.Empty;
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
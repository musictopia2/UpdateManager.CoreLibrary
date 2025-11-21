namespace UpdateManager.CoreLibrary.Utilities;
internal static class LibraryDependencyResolver<T>
    where T : ILibraryUpgradeModel, new()
{
    public static BasicList<T> ResolveDependencies(BasicList<T> libraries)
    {
        // Process the dependencies for each library
        foreach (T library in libraries)
        {
            library.Dependencies = GetDependencies(library, libraries);
            library.Dependencies.RemoveAllOnly(x => x == library.PackageName); // Can't depend on itself.
        }
        FinishScan(libraries);
        // Return the updated list with dependencies populated
        return libraries;
    }

    // Helper method to get dependencies from XML for each library
    private static BasicList<string> GetDependencies(T library, BasicList<T> list)
    {
        XElement source = XElement.Load(library.CsProjPath);
        BasicList<string> dependencies = [];
        var packageReferences = source.Descendants("PackageReference")
                                      .Where(x => x.Attribute("Include") != null)
                                      .Select(x => x.Attribute("Include")!.Value!)
                                      .ToBasicList();
        foreach (var depName in packageReferences)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(depName, library.PackageName) &&
                list.Any(l => StringComparer.OrdinalIgnoreCase.Equals(l.PackageName, depName)))
            {
                dependencies.Add(depName);
            }
        }
        return dependencies;
    }
    private static void FinishScan(BasicList<T> list)
    {
        bool changesMade;
        // Create a dictionary for faster lookup by library name (ignoring case)
        var libraryDict = list.ToDictionary(l => l.PackageName, StringComparer.OrdinalIgnoreCase);

        // Continue scanning until no more changes are made (i.e., all dependencies are resolved)
        do
        {
            changesMade = false;

            // Iterate through each library in the list
            foreach (var library in list)
            {
                // To hold new dependencies that need to be added to the current library
                var newDependencies = new List<string>();

                // For each dependency in the library's Dependencies list
                foreach (var dep in library.Dependencies)
                {
                    // Now we need to resolve indirect dependencies by looking at other libraries in the list
                    if (libraryDict.TryGetValue(dep, out var depLibrary))
                    {
                        // If the dependency library has its own dependencies, we need to add them too
                        foreach (var indirectDep in depLibrary.Dependencies)
                        {
                            // Only add indirect dependencies if they are not already in the current library's list
                            if (!library.Dependencies.Contains(indirectDep, StringComparer.OrdinalIgnoreCase))
                            {
                                newDependencies.Add(indirectDep);
                                changesMade = true; // Mark that a change was made
                            }
                        }
                    }
                }

                // Add the new dependencies to the library's Dependencies list
                foreach (var newDep in newDependencies)
                {
                    if (!library.Dependencies.Contains(newDep, StringComparer.OrdinalIgnoreCase))
                    {
                        library.Dependencies.Add(newDep);
                        changesMade = true;
                    }
                }
            }

        } while (changesMade); // Repeat the process until no changes are made
    }
}
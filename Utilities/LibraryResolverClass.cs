namespace UpdateManager.CoreLibrary.Utilities;
public static class LibraryResolverClass<T>
    where T: ILibraryUpgradeModel, new()
{

    public static BasicList<T> GetCompleteResolvedLibraries(BasicList<T> libraries)
    {
        var output = LibraryDependencyResolver<T>.ResolveDependencies(libraries);
        return output;
    }

    public static BasicList<T> GetBuildOrder(BasicList<T> libraries, string rootName)
    {
        var sortedLibraries = new BasicList<T>();
        var processedLibraries = new HashSet<string>(); // Keeps track of libraries that have been processed

        // Start the recursion with the root library
        AddLibraryToBuildOrder(rootName, libraries, sortedLibraries, processedLibraries);

        // Now loop through the rest of the libraries that weren't visited as dependencies
        foreach (var library in libraries)
        {
            if (!processedLibraries.Contains(library.PackageName))
            {
                AddLibraryToBuildOrder(library.PackageName, libraries, sortedLibraries, processedLibraries);
            }
        }

        // Ensure that all libraries are processed and none are missed
        if (sortedLibraries.Count != libraries.Count)
        {
            throw new InvalidOperationException($"The number of libraries in the sorted order ({sortedLibraries.Count}) does not match the number of libraries in the input list ({libraries.Count}).");
        }

        return sortedLibraries;
    }
    private static void AddLibraryToBuildOrder(string rootName, BasicList<T> libraries, BasicList<T> sortedLibraries, HashSet<string> processedLibraries)
    {
        // Skip if already processed
        if (processedLibraries.Contains(rootName))
        {
            return;
        }

        // Find the library by name
        var library = libraries.FirstOrDefault(l => l.PackageName == rootName);
        if (library == null)
        {
            return; // If no library is found, do nothing (you could throw an exception here if it's unexpected)
        }

        // Mark the library as processed
        processedLibraries.Add(rootName);

        // Add dependencies first (recursive call)
        foreach (var dependency in library.Dependencies)
        {
            AddLibraryToBuildOrder(dependency, libraries, sortedLibraries, processedLibraries);
        }

        // After processing dependencies, add the current library to the sorted list
        sortedLibraries.Add(library);
    }
}
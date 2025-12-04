namespace UpdateManager.CoreLibrary.Utilities;
public static class LibraryDependencyResolver<T>
    where T : ILibraryUpgradeModel, new()
{
    public static BasicList<T> ResolveDependencies(
        BasicList<T> libraries,
        Func<string, string>? normalizer = null)
    {
        // Process the dependencies for each library
        foreach (T library in libraries)
        {
            library.Dependencies = GetDependencies(library, libraries, normalizer);
            library.Dependencies.RemoveAllOnly(x =>
                StringComparer.OrdinalIgnoreCase.Equals(x, Normalize(library.PackageName, normalizer)));
        }

        FinishScan(libraries, normalizer);

        return libraries;
    }

    private static string Normalize(string name, Func<string, string>? normalizer)
    {
        return normalizer?.Invoke(name) ?? name;
    }

    // Helper method to get dependencies from XML for each library
    private static BasicList<string> GetDependencies(
        T library,
        BasicList<T> list,
        Func<string, string>? normalizer)
    {
        XElement source = XElement.Load(library.CsProjPath);
        BasicList<string> dependencies = [];

        var packageReferences = source.Descendants("PackageReference")
                                      .Where(x => x.Attribute("Include") != null)
                                      .Select(x => x.Attribute("Include")!.Value!)
                                      .ToBasicList();

        string normalizedLibraryName = Normalize(library.PackageName, normalizer);

        foreach (var rawName in packageReferences)
        {
            string depName = Normalize(rawName, normalizer);

            if (StringComparer.OrdinalIgnoreCase.Equals(depName, normalizedLibraryName))
                continue;

            if (list.Any(l =>
                StringComparer.OrdinalIgnoreCase.Equals(
                    Normalize(l.PackageName, normalizer),
                    depName)))
            {
                dependencies.Add(depName);
            }
        }

        return dependencies;
    }

    private static void FinishScan(
        BasicList<T> list,
        Func<string, string>? normalizer)
    {
        bool changesMade;

        // Create dictionary using NORMALIZED names
        var libraryDict = list.ToDictionary(
            l => Normalize(l.PackageName, normalizer),
            StringComparer.OrdinalIgnoreCase);

        do
        {
            changesMade = false;

            foreach (var library in list)
            {
                var newDependencies = new List<string>();

                foreach (var dep in library.Dependencies)
                {
                    string normalizedDep = Normalize(dep, normalizer);

                    if (libraryDict.TryGetValue(normalizedDep, out var depLibrary))
                    {
                        foreach (var indirectDep in depLibrary.Dependencies)
                        {
                            if (!library.Dependencies.Contains(indirectDep, StringComparer.OrdinalIgnoreCase))
                            {
                                newDependencies.Add(indirectDep);
                                changesMade = true;
                            }
                        }
                    }
                }

                foreach (var newDep in newDependencies)
                {
                    if (!library.Dependencies.Contains(newDep, StringComparer.OrdinalIgnoreCase))
                    {
                        library.Dependencies.Add(newDep);
                        changesMade = true;
                    }
                }
            }

        } while (changesMade);
    }
}
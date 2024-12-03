namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
public static class ExcludedDependencies
{
    // List for packages excluded entirely from updates (no updates will be considered)
    public static readonly HashSet<string> ExcludedPackagesAll = [];

    // List for packages excluded from minor/patch updates, but still allow major updates
    public static readonly HashSet<string> ExcludedExceptMajor = [];
}
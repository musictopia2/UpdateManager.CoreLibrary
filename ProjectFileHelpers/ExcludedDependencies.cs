namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
public static class ExcludedDependencies
{
    // List for packages excluded entirely from updates (no updates will be considered)
    public static HashSet<string> ExcludedPackagesAll { get; set; } = [];

    // List for packages excluded from minor/patch updates, but still allow major updates
    public static HashSet<string> ExcludedExceptMajor { get; set; } = [];
}
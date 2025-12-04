namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
public static class ExcludedDependencies
{
    

    // List for packages excluded from minor/patch updates, but still allow major updates
    public static HashSet<string> ExcludedExceptMajor { get; set; } = [];

    public static Dictionary<string, string> ForcedVersions { get; set; } = [];
    // delegate signature:
    // return true => skip updating this package for this project
    // return false => update normally
    public static Func<string, string, bool>? PackageSkipRule { get; set; }

    //not sure if it should be a delegate or not (?)


}
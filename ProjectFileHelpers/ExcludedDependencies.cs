namespace UpdateManager.CoreLibrary.ProjectFileHelpers;
public static class ExcludedDependencies
{
    // List for packages excluded entirely from updates (no updates will be considered)
    public static HashSet<string> ExcludedPackagesAll { get; set; } = [];

    // List for packages excluded from minor/patch updates, but still allow major updates
    public static HashSet<string> ExcludedExceptMajor { get; set; } = [];

    public static Dictionary<string, string> ForcedVersions { get; set; } = [];

    //public static Dictionary<string, string> ForcedVersions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    //{
    //    { "Microsoft.AspNetCore.Components.WebView.Wpf", "9.0.120" },
    //    // Add more if needed
    //    // { "Some.Other.Package", "1.2.3" }
    //};

}
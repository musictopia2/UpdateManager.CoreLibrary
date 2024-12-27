namespace UpdateManager.CoreLibrary.Models;
public class NuGetPackageModel : IPackageVersionable, INugetModel
{
    public string PackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public EnumTargetFramework Framework { get; set; } = EnumTargetFramework.NetRuntime;
    public string CsProjPath { get; set; } = ""; //i think this makes the most sense here.
    public string NugetPackagePath { get; set; } = "";
    public EnumFeedType FeedType { get; set; }
    /// <summary>
    /// Indicates whether the package should be excluded from processing in the current workflow.
    /// 
    /// When set to <c>true</c>, the package is ignored in processes such as discovery, upgrades, and deployment.
    /// This property is useful for:
    /// - **Temporarily** excluding a package from processing (e.g., under maintenance, not required for a specific task).
    /// - **Permanently** excluding a package (e.g., replacing an old version, no longer needed).
    ///
    /// Example usage:
    /// - In package discovery or update processes, excluded packages will not be included in the list.
    /// - During build or deployment, excluded packages will not be processed.
    ///
    /// Set to <c>true</c> to exclude the package; <c>false</c> to include it in the process.
    /// </summary>
    public bool IsExcluded { get; set; }
    public bool Development { get; set; } = false; // if this is in development, then can put in a development feed.
    public string PrefixForPackageName { get; set; } = "";
}
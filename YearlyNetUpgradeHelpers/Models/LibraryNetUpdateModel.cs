namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Models;
public class LibraryNetUpdateModel : IPackageVersionable, INugetModel
{
    public string PackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public EnumDotNetUpgradeStatus Status { get; set; } = EnumDotNetUpgradeStatus.None;
    public string CsProjPath { get; set; } = ""; //i think this makes the most sense here.
    public string NugetPackagePath { get; set; } = "";
    public BasicList<string> Dependencies { get; set; } = []; // List of library names this library depends on
    public EnumFeedType PackageType { get; set; } // Type of package: Local, Public
    public bool Development { get; set; } //this is needed so if development, can't post to nuget.
}
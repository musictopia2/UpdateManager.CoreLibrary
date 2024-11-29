namespace UpdateManager.CoreLibrary.GeneralHelpers.Models;
public class NuGetPackageModel : IPackageVersionable
{
    public string PackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public EnumTargetFramework Framework { get; set; } = EnumTargetFramework.NetRuntime;
    public string CsProjPath { get; set; } = ""; //i think this makes the most sense here.
    public string NugetPackagePath { get; set; } = "";
    public EnumFeedType FeedType { get; set; }
    public bool TemporarilyIgnore { get; set; } //i think this should be used as well.  so i reserve the right to ignore this one.
}
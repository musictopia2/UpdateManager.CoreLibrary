namespace UpdateManager.CoreLibrary.Models;
public class NuGetTemplateModel : INugetModel
{
    public string CsProjPath { get; set; } = "";
    public string NugetPackagePath { get; set; } = "";
    public bool Development { get; set; }
    public string PackageName { get; set; } = "";
    public string PrefixForPackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public bool IsExcluded { get; set; }
    public EnumFeedType FeedType { get; set; }
}
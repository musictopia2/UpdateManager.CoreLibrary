namespace UpdateManager.CoreLibrary.Models;
public class NuGetTemplateModel : IVersionable
{
    //public string CsProjPath { get; set; } = "";
    public string NugetPackagePath { get; set; } = "";
    public string Directory { get; set; } = "";
    public bool Development { get; set; }
    public string PackageName { get; set; } = "";
    public string PrefixForPackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public bool IsExcluded { get; set; }
    public EnumFeedType FeedType { get; set; }
    //if its null, then never has been updated.
    public DateTime? LastUpdated { get; set; } //this means if there are no changes to the files when i rerun the program, then this won't even run.

}
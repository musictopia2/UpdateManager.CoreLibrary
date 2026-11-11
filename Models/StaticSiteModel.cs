namespace UpdateManager.CoreLibrary.Models;
public class StaticSiteModel
{
    public string ProjectPath { get; set; } = "";


    // Identifies the shared host configuration.
    public string DeploymentGroup { get; set; } = "";

    // Published subfolder, such as "scoring".
    public string Folder { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string Version { get; set; } = "";

    public bool Disabled { get; set; }
}
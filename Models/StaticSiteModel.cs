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
    public bool HostUpdatePending { get; set; }

    // Last source commit successfully submitted by management.
    // This does not mean Render has finished deploying it.
    public string LastSubmittedCommit { get; set; } = "";

    // A version has been allocated, but source submission is unfinished.
    public bool SourceUpdatePending { get; set; }

    // Updated source has been pushed, but its coordinator build
    // still needs to be requested.
    public bool DeploymentBuildPending { get; set; }

    public bool RenderCacheClearPending { get; set; }

}
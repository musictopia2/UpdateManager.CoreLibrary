namespace UpdateManager.CoreLibrary.Models;
public class StaticSiteDeploymentGroupModel
{
    // Matches StaticSiteModel.DeploymentGroup.
    public string Name { get; set; } = "";

    // Local coordinator repository containing apps.txt.
    public string RepositoryDirectory { get; set; } = "";
    public string WorkflowFile { get; set; } = "deploy.yml";
}
namespace UpdateManager.CoreLibrary.Models;
public sealed class LocalStaticSiteDeploymentSettings
{
    public required string ContainerName { get; init; }
    public required string ContainerWebRoot { get; init; }
    public required string StagingRoot { get; init; }
    public required string ExpectedReleasesRoot { get; init; }
}
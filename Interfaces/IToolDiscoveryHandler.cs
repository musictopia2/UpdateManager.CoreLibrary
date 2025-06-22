namespace UpdateManager.CoreLibrary.Interfaces;
public interface IToolDiscoveryHandler
{
    Task<BasicList<string>> GetToolDirectoriesAsync();
    bool CanIncludeProject(string projectPath);
    void CustomizePackageModel(NuGetToolModel tool);
    bool NeedsPrefix(NuGetToolModel tool, CsProjEditor editor);
    //this is needed though.
    EnumFeedType GetFeedType(string projectPath);
}
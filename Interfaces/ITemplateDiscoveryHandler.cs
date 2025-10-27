namespace UpdateManager.CoreLibrary.Interfaces;
public interface ITemplateDiscoveryHandler
{
    Task<BasicList<string>> GetTemplateDirectoriesAsync();
    bool CanIncludeProject(string projectPath);
    void CustomizePackageModel(NuGetTemplateModel template);
    bool NeedsPrefix(NuGetTemplateModel template);
    //this is needed though.
    EnumFeedType GetFeedType(string projectPath);
}
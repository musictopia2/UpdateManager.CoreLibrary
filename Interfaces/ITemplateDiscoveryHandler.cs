namespace UpdateManager.CoreLibrary.Interfaces;
public interface ITemplateDiscoveryHandler
{
    Task<BasicList<string>> GetTemplateDirectoriesAsync();
    bool CanIncludeProject(string directoryPath);
    void CustomizePackageModel(NuGetTemplateModel template);
    bool NeedsPrefix(NuGetTemplateModel template);
    //this is needed though.
    EnumFeedType GetFeedType(string directoryPath);
}
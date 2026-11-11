namespace UpdateManager.CoreLibrary.Interfaces;
public interface IStaticSiteDiscoveryHandler
{
    Task<BasicList<string>> GetStaticSiteDirectoriesAsync();
    bool CanIncludeProject(string projectPath);
    void CustomizeStaticSiteModel(StaticSiteModel site);
}
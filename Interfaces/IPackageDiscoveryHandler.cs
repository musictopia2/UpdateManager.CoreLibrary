namespace UpdateManager.CoreLibrary.Interfaces;
public interface IPackageDiscoveryHandler
{
    Task<BasicList<string>> GetPackageDirectoriesAsync();
    bool CanIncludeProject(string projectPath);
    void CustomizePackageModel(NuGetPackageModel package);
    //this is needed though.
    EnumFeedType GetFeedType(string packageName);
}
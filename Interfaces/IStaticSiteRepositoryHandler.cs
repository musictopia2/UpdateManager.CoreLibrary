namespace UpdateManager.CoreLibrary.Interfaces;
public interface IStaticSiteRepositoryHandler
{
    string Owner { get; }
    string DefaultBranch { get; }
    string GetRepositoryName(string projectPath);
}
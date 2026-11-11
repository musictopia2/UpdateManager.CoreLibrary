namespace UpdateManager.CoreLibrary.Interfaces;
public interface IStaticSitesContext
{
    // Includes disabled sites so discovery does not add them again.
    Task<BasicList<StaticSiteModel>> GetStaticSitesAsync();

    Task AddStaticSiteAsync(StaticSiteModel site);

    // Identifies the existing registration by ProjectPath.
    Task UpdateStaticSiteAsync(StaticSiteModel site);
    Task UpdateStaticSiteVersionAsync(string projectPath, string version);
    Task SaveCompleteListAsync(BasicList<StaticSiteModel> sites);
}
namespace UpdateManager.CoreLibrary.DataAccess;
public class FileStaticSitesContext : IStaticSitesContext
{
    private readonly string _sitesPath =
        bb1.Configuration!.RequiredStaticSitesPath;

    private BasicList<StaticSiteModel> _list = [];

    public static Func<string, Task>? AfterSaveAsync { get; set; }

    private async Task LoadAsync()
    {
        if (ff1.FileExists(_sitesPath) == false)
        {
            _list = [];
            return;
        }

        _list = await jj1.RetrieveSavedObjectAsync<
            BasicList<StaticSiteModel>>(_sitesPath);
    }

    private async Task SaveAsync()
    {
        string directory = Path.GetDirectoryName(_sitesPath)!;
        Directory.CreateDirectory(directory);

        await jj1.SaveObjectAsync(_sitesPath, _list);

        if (AfterSaveAsync is { } afterSave)
        {
            await afterSave(_sitesPath);
        }
    }

    private StaticSiteModel FindSite(string projectPath)
    {
        return _list.SingleOrDefault(x =>
            string.Equals(
                x.ProjectPath,
                projectPath,
                StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Static site '{projectPath}' was not found.");
    }

    async Task<BasicList<StaticSiteModel>>
        IStaticSitesContext.GetStaticSitesAsync()
    {
        await LoadAsync();
        return _list.ToBasicList();
    }

    async Task IStaticSitesContext.AddStaticSiteAsync(
        StaticSiteModel site)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(site.ProjectPath);

        await LoadAsync();

        if (_list.Any(x => string.Equals(
            x.ProjectPath,
            site.ProjectPath,
            StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Static site '{site.ProjectPath}' is already registered.");
        }

        _list.Add(site);
        await SaveAsync();
    }

    async Task IStaticSitesContext.UpdateStaticSiteAsync(
        StaticSiteModel site)
    {
        await LoadAsync();

        StaticSiteModel existing = FindSite(site.ProjectPath);

        existing.DeploymentGroup = site.DeploymentGroup;
        existing.Folder = site.Folder;
        existing.DisplayName = site.DisplayName;
        existing.Version = site.Version;
        existing.Disabled = site.Disabled;

        await SaveAsync();
    }

    async Task IStaticSitesContext.UpdateStaticSiteVersionAsync(
        string projectPath,
        string version)
    {
        await LoadAsync();

        StaticSiteModel existing = FindSite(projectPath);
        existing.Version = version;

        await SaveAsync();
    }

    async Task IStaticSitesContext.SaveCompleteListAsync(
        BasicList<StaticSiteModel> sites)
    {
        _list = sites.ToBasicList();
        await SaveAsync();
    }
}
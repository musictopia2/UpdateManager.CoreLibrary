namespace UpdateManager.CoreLibrary.DataAccess;
public class FileAppsContext : IAppsContext
{
    readonly string _appsPath = bb1.Configuration!.RequiredAppPackagesPath;
    private BasicList<AppModel> _list = [];
    async Task IAppsContext.AddAppAsync(AppModel app)
    {
        _list.Add(app);
        await SaveAsync();
    }
    private async Task SaveAsync()
    {
        await jj1.SaveObjectAsync(_appsPath, _list);
    }
    async Task<BasicList<AppModel>> IAppsContext.GetAllAppsAsync()
    {
        await GetAysnc();
        
        return _list.ToBasicList();
    }
    private async Task GetAysnc()
    {
        _list.Clear();
        if (ff1.FileExists(_appsPath) == false)
        {
            return;
        }
        _list = await jj1.RetrieveSavedObjectAsync<BasicList<AppModel>>(_appsPath);
    }
    async Task<BasicList<AppModel>> IAppsContext.GetMaintainedAppsAsync()
    {
        await GetAysnc();
        return _list.Where(x => x.Deleted == false).ToBasicList();
    }
    async Task IAppsContext.UpdateAppAsync(AppModel app)
    {
        var update = _list.SingleOrDefault(x => x.ProjectName == app.ProjectName)
            ?? throw new InvalidOperationException($"Package '{app.ProjectName}' not found.");
        update.NetworkedPath = app.NetworkedPath;
        update.ShortcutName = app.ShortcutName;
        update.Deleted = app.Deleted;
        //i think updating should be just these 3.
        await SaveAsync();
    }
}
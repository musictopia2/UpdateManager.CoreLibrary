namespace UpdateManager.CoreLibrary.Interfaces;
public interface IAppsContext
{
    // Retrieves the list of all apps.
    Task<BasicList<AppModel>> GetAllAppsAsync();
    //this will show the apps that are maintained (deleted = false)
    Task<BasicList<AppModel>> GetMaintainedAppsAsync();

    // Updates the properties of an app with the provided model.
    Task UpdateAppAsync(AppModel app);

    // Adds a new app to the list.
    Task AddAppAsync(AppModel app);
}
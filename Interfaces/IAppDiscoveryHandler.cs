namespace UpdateManager.CoreLibrary.Interfaces;
public interface IAppDiscoveryHandler
{
    // Discover all directories where apps are stored
    Task<BasicList<string>> GetAppDirectoriesAsync();

    // Decide if an app should be included based on its directory or path
    bool CanIncludeApp(string appPath);

    // Customize the AppModel with additional metadata, such as network paths or shortcut names
    void CustomizeAppModel(AppModel app);
}
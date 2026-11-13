namespace UpdateManager.CoreLibrary.Interfaces;
public interface IStaticSiteSetupHandler
{
    Task<bool> IsProjectConfiguredAsync(string projectPath);

    // Returns Razor markup connected to the generated Refresh method.
    string GetUpdateButtonMarkup();
}
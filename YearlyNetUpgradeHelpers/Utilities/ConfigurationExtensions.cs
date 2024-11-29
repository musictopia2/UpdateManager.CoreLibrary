namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Utilities;
public static class ConfigurationExtensions
{
    public static string GetLibraryPath(this IConfiguration configuration)
    {
        string? value = configuration[NetUpgradeConfigurationKeys.LibraryPath];
        if (value is null)
        {
            throw new ConfigurationKeyNotFoundException("The library path not found");
        }
        return value;
    }
    public static string GetNetPath(this IConfiguration configuration)
    {
        string? value = configuration[NetUpgradeConfigurationKeys.NetPath];
        if (value is null)
        {
            throw new ConfigurationKeyNotFoundException("The net library path not found");
        }
        return value;
    }
}
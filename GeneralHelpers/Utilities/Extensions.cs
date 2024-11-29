namespace UpdateManager.CoreLibrary.GeneralHelpers.Utilities;
public static class Extensions
{
    public static string GetRequiredNuGetPackagesPath(this IConfiguration configuration)
    {
        // Check if the configuration key is found
        var value = configuration[NuGetPackagesConfigurationKeys.NuGetPackagesKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The NuGet package path key is not registered in the configuration.");
        }
        return value;
    }
}
namespace UpdateManager.CoreLibrary.Utilities;
public static class Extensions
{
    public static string GetPackageID(this IPackageVersionable package)
    {
        if (string.IsNullOrWhiteSpace(package.PrefixForPackageName))
        {
            return package.PackageName;
        }
        return $"{package.PrefixForPackageName}.{package.PackageName}";
    }
    public static string GetFeedPostProcessorProgramFromConfig(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.PostBuildFeedProcessorKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The post program is not registered in the configuration.");
        }
        return value;
    }
    public static string GetPackagePrefixFromConfig(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.PrefixKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The package prefix key is not registered in the configuration.");
        }
        return value;
    }
    public static string GetDevelopmentPackagePath(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.DevelopmentPackageKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The development nuget package feed path key is not registered in the configuration.");
        }
        return value;
    }
    public static string GetStagingPackagePath(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.StagingPackageKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The staging nuget package feed path key is not registered in the configuration.");
        }
        return value;
    }
    public static string GetPrivatePackagePath(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.PrivatePackageKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The private nuget package feed path key is not registered in the configuration.");
        }
        return value;
    }
    public static string GetRequiredNuGetPackagesPath(this IConfiguration configuration)
    {
        // Check if the configuration key is found
        var value = configuration[UpdateSystemConfigurationKeys.CustomPackageInfoKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The NuGet package path key is not registered in the configuration.");
        }
        return value;
    }
    public static string GetNugetConfigPath(this IConfiguration configuration)
    {
        var value = configuration[UpdateSystemConfigurationKeys.PackageConfigFileKey];

        // If the key is not found, throw an exception
        if (string.IsNullOrEmpty(value))
        {
            throw new ConfigurationKeyNotFoundException("The NuGet config path is not registered in the configuration.");
        }
        return value;
    }
    // Extract the major version from version string
    public static int GetMajorVersiont(this string version)
    {
        var versionParts = version.Split('.');
        if (versionParts.Length < 1)
        {
            throw new CustomBasicException("Invalid version format. Major version is required.");
        }
        return int.Parse(versionParts[0]);
    }
    // Extract the minor version (the last part of the version string)
    public static int GetMinorVersion(this string version)
    {
        var versionParts = version.Split('.');
        if (versionParts.Length < 1)
        {
            throw new CustomBasicException("Invalid version format. Minor version is required.");
        }

        // The very last part should always be the minor version, regardless of how many parts are in the version string
        return int.Parse(versionParts.Last());
    }
    // Increment the minor version and maintain the major.0.minor format
    public static string IncrementMinorVersion(this string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new CustomBasicException("Version string is empty or null, unable to increment minor version.");
        }

        var versionParts = version.Split('.');
        // Ensure that the version has at least 2 parts (major.minor format)
        if (versionParts.Length < 2)
        {
            throw new CustomBasicException("Invalid version format. Version should have at least 'major.minor' format.");
        }
        // Extract major version (the first part)
        int major = int.Parse(versionParts[0]);

        // Always take the last part as the minor version, no matter how many parts are there
        int minor = int.Parse(versionParts.Last());

        // Increment minor version
        minor++;

        // Return in major.0.minor format
        return $"{major}.0.{minor}";
    }

}
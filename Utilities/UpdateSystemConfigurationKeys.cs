namespace UpdateManager.CoreLibrary.Utilities;
public static class UpdateSystemConfigurationKeys
{
    // Key for custom package information (name, version, csproj, etc.)
    public static string CustomPackageInfoKey => "CustomPackageInfoKey";

    // Key for configuration file path (e.g., path to the NuGet.config or other config files)
    public static string PackageConfigFileKey => "PackageConfigFileKey";
    // Key for configuration file path for the Private Nuget Packages
    public static string PrivatePackageKey => "PackagePackageKey";
    public static string DevelopmentPackageKey => "DevelopmentPackageKey";
    public static string StagingPackageKey => "StagingPackageKey";
}
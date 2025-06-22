namespace UpdateManager.CoreLibrary.Utilities;
public static class UpdateSystemConfigurationKeys
{
    // Key for custom package information (name, version, csproj, etc.)
    public static string CustomPackageInfoKey => "CustomPackageInfoKey";
    public static string CustomToolInfoKey => "CustomToolInfoKey";

    // Key for configuration file path for managing apps.
    public static string AppInfoKey => "AppInfoKey";

    // Key for configuration file path (e.g., path to the NuGet.config or other config files)
    public static string PackageConfigFileKey => "PackageConfigFileKey";
    

    // Key for configuration file path for the Private Nuget Packages
    public static string PrivatePackageKey => "PrivatePackageKey";
    public static string DevelopmentPackageKey => "DevelopmentPackageKey";
    public static string StagingPackageKey => "StagingPackageKey";
    public static string PrefixKey => "PrefixKey";
    public static string PostBuildFeedProcessorKey_Packages => "PostBuildFeedProcessorKey_Packages";
    public static string PostBuildFeedProcessorKey_Tools => "PostBuildFeedProcessorKey_Tools";
    public static string UploadedPackagesStoragePathKey => "UploadedPackagesStoragePathKey";
    public static string UploadedToolsStoragePathKey => "UploadedtoolsStoragePathKey";
}
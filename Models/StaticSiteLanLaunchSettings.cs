namespace UpdateManager.StaticSiteManagementCoreLibrary;
public sealed record StaticSiteLanLaunchSettings(
    string ApplicationUrl,
    string LaunchUrl,
    string CertificatePath,
    string KeyPath);

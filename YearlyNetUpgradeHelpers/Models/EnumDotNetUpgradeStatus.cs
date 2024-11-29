namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Models;
public enum EnumDotNetUpgradeStatus
{
    None,                // Waiting to be processed
    Built,               // Successfully built
    Published,           // Successfully published to local feed
    WaitingForNuGet,     // Successfully published locally, waiting to publish on NuGet
    WaitingForGitHub,    // Published on NuGet but waiting to commit on GitHub
    Completed            // All steps completed (including GitHub commit)
}
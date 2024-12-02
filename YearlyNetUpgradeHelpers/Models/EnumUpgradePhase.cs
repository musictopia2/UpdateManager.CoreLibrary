namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Models;
public enum EnumUpgradePhase
{
    NotStarted,      // Process hasn't begun yet
    PendingUpdate,   // Needs an upgrade
    InProgress,      // Upgrade is running
    UpgradeCompleted // Basic upgrade is done, post-process might still run
}
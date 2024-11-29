namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Models;
public enum EnumDotNetUpdateStatus
{
    None,
    Start, //this means you are starting to begin with.
    NeedsUpdate,
    NoFeedsSoFar, //this means you had no feeds so far.
    //CommitGamePackageAlone,
    Completed
}
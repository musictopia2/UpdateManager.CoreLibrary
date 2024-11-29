namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Utilities;
public static class LibraryInclusionGlobals
{
    /// <summary>
    /// if nothing is specified, then includes all and not just what is requested
    /// </summary>
    public static HashSet<string> LibrariesToIncludeForTest { get; set; } = [];
}
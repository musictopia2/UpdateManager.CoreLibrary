namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Extensions;
internal static class DotNetUpgradeExtensions
{
    public static bool NeedsToUpdateVersion(this DotNetVersionUpgradeModel config, IDateOnlyPicker date)
    {
        DateOnly currentDate = date.GetCurrentDate;
        DateOnly lastUpdate = config.LastUpdated;
        DateOnly dateNeeded;
        int yearNeeded;
        if (currentDate.Year == lastUpdate.Year)
        {
            yearNeeded = lastUpdate.Year + 1;
        }
        else
        {
            yearNeeded = currentDate.Year;
        }
        dateNeeded = new(yearNeeded, 11, 1); //november 1 at the least.
        if (currentDate >= dateNeeded)
        {
            return true;
        }
        return false;
    }
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Extensions;
public static class CustomProcessesExtensions
{
    public static void CheckForTesting(this DotNetVersionUpgradeModel model)
    {
        if (model.IsTestMode)
        {
            throw new CustomBasicException("Cannot run custom processes because its in test mode");
        }
    }
}
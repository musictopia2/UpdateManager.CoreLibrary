namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface IPreUpgradeProcessHandler
{
    // Asynchronous method to initialize any necessary data or resources for pre-upgrade tasks
    Task InitAsync();

    // Determines if any pre-upgrade processes are needed
    bool ArePreUpgradeProcessesNeeded();

    // Executes the pre-upgrade processes (like updating the program to .NET 10 without modifying dependencies)
    Task<bool> RunPreUpgradeProcessesAsync();

    // Resets any flags or state for a new version (this could be necessary to reset before the pre-upgrade starts)
    Task ResetFlagsForNewVersionAsync();
}
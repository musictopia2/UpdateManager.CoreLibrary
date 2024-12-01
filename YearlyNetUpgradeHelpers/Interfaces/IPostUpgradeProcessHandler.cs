namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IPostUpgradeProcessHandler
{
    // Asynchronous method to initialize any necessary data or resources
    Task InitAsync();

    // Determines if any custom processes are needed
    bool ArePostUpgradeProcessesNeeded();

    // Resets any flags or state for a new version
    Task ResetFlagsForNewVersionAsync();

    // Executes custom processes and returns a bool indicating success
    Task<bool> RunPostUpgradeProcessesAsync();

    Task<bool> HandleCommitAsync(LibraryNetUpdateModel netUpdateModel);
}
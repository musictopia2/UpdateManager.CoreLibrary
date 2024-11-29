namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface ICustomProcessHandler
{
    // Asynchronous method to initialize any necessary data or resources
    Task InitAsync();

    // Determines if any custom processes are needed
    bool AreCustomProcessesNeeded();

    // Resets any flags or state for a new version
    Task ResetFlagsForNewVersionAsync();

    // Executes custom processes and returns a bool indicating success
    Task<bool> RunCustomProcessesAsync();

    Task<bool> HandleCommitAsync(LibraryNetUpdateModel netUpdateModel);
}
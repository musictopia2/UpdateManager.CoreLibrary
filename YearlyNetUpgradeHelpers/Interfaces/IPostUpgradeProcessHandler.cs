namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface IPostUpgradeProcessHandler
{
    // Asynchronous method to initialize any necessary data or resources
    Task InitAsync(DotNetVersionUpgradeModel dotNetVersion);

    // Determines if any custom processes are needed
    bool ArePostUpgradeProcessesNeeded(DotNetVersionUpgradeModel dotNetVersion);

    // Resets any flags or state for a new version
    Task ResetFlagsForNewVersionAsync(DotNetVersionUpgradeModel dotNetVersion);

    // Executes custom processes and returns a bool indicating success
    Task<bool> RunPostUpgradeProcessesAsync(DotNetVersionUpgradeModel dotNetVersion);

    Task<bool> HandleCommitAsync(LibraryNetUpdateModel netUpdateModel, DotNetVersionUpgradeModel dotNetVersion);
}
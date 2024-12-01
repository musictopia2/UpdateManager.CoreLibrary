using UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;

namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class NoCustomProcessesHandler : IPostUpgradeProcessHandler, IPreUpgradeProcessHandler
{
    Task IPostUpgradeProcessHandler.ResetFlagsForNewVersionAsync()
    {
        return Task.CompletedTask;
    }
    bool IPostUpgradeProcessHandler.ArePostUpgradeProcessesNeeded()
    {
        return false;  // No post-upgrade processes needed
    }
    Task<bool> IPostUpgradeProcessHandler.HandleCommitAsync(LibraryNetUpdateModel netUpdateModel)
    {
        // Return false when no post-upgrade processes are needed
        return Task.FromResult(false);  // No custom commit handling needed.
    }
    Task IPostUpgradeProcessHandler.InitAsync()
    {
        return Task.CompletedTask;
    }
    Task<bool> IPostUpgradeProcessHandler.RunPostUpgradeProcessesAsync()
    {
        // Throw an exception if post-upgrade processes are triggered unexpectedly
        throw new InvalidOperationException("Post-upgrade processes were called, but none are configured. This should not happen.");
    }
    Task IPreUpgradeProcessHandler.InitAsync()
    {
        return Task.CompletedTask;
    }
    bool IPreUpgradeProcessHandler.ArePreUpgradeProcessesNeeded()
    {
        return false;  // No pre-upgrade processes needed
    }
    Task<bool> IPreUpgradeProcessHandler.RunPreUpgradeProcessesAsync()
    {
        // Throw an exception if pre-upgrade processes are triggered unexpectedly
        throw new InvalidOperationException("Pre-upgrade processes were called, but none are configured. This should not happen.");
    }
    Task IPreUpgradeProcessHandler.ResetFlagsForNewVersionAsync()
    {
        return Task.CompletedTask;  // No flags to reset for the new version
    }
}
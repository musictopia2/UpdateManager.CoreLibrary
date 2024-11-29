namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class NoCustomProcessesHandler : ICustomProcessHandler
{
    bool ICustomProcessHandler.AreCustomProcessesNeeded()
    {
        return false;
    }

    Task<bool> ICustomProcessHandler.HandleCommitAsync(LibraryNetUpdateModel netUpdateModel)
    {
        return Task.FromResult(false); //this means i did not decide to handle later.
    }

    
    Task ICustomProcessHandler.InitAsync()
    {
        return Task.CompletedTask;
    }

    Task ICustomProcessHandler.ResetFlagsForNewVersionAsync()
    {
        return Task.CompletedTask;
    }

    Task<bool> ICustomProcessHandler.RunCustomProcessesAsync()
    {
        throw new CustomBasicException("Should not have ran the custom processes because there was none even needed");
    }
}
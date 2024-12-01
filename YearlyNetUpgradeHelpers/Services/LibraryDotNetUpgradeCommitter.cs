using UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;

namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class LibraryDotNetUpgradeCommitter(IPostUpgradeProcessHandler handler) : ILibraryDotNetUpgradeCommitter
{
    async Task<bool> ILibraryDotNetUpgradeCommitter.CommitAndPushToGitHubAsync(LibraryNetUpdateModel updateModel, DotNetVersionUpgradeModel versionUpgradeModel, CancellationToken cancellationToken)
    {
        bool rets;
        rets = await handler.HandleCommitAsync(updateModel);
        if (rets)
        {
            return true; //because someone else is handling this later.
        }
        string version = bb1.Configuration!.GetNetPath();
        rets = await GitHubCommitter.CommitAndPushToGitHubAsync(updateModel.CsProjPath, $"Updated To {version}", cancellationToken);
        return rets;
    }
}
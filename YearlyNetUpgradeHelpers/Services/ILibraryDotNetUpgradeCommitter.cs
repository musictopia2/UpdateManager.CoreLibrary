namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public interface ILibraryDotNetUpgradeCommitter
{
    Task<bool> CommitAndPushToGitHubAsync(LibraryNetUpdateModel updateModel, DotNetVersionUpgradeModel versionUpgradeModel, CancellationToken cancellationToken = default);
}
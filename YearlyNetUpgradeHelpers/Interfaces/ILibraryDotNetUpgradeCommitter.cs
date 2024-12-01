namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces;
public interface ILibraryDotNetUpgradeCommitter
{
    Task<bool> CommitAndPushToGitHubAsync(LibraryNetUpdateModel updateModel, DotNetVersionUpgradeModel versionUpgradeModel, CancellationToken cancellationToken = default);
}
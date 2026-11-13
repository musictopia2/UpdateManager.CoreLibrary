namespace UpdateManager.CoreLibrary.Interfaces;
public interface IRequiredPackagePublicationService
{
    Task EnsurePublishedAsync(
        BasicList<NuGetPackageModel> packages,
        CancellationToken cancellationToken = default);
}
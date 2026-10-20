namespace UpdateManager.CoreLibrary.NugetHelpers;

public interface INugetUploader
{
    Task<bool> UploadNugetPackageAsync(
        string nugetFilePath,
        string packageId,
        string packageVersion,
        CancellationToken cancellationToken = default);
}
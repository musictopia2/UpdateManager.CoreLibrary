namespace UpdateManager.CoreLibrary.GeneralHelpers.NugetHelpers;
public interface INugetUploader
{
    Task<bool> UploadNugetPackageAsync(string nugetFile, CancellationToken cancellationToken = default); //this means there can even be a private version of this.
}
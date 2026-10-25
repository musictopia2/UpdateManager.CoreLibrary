namespace UpdateManager.CoreLibrary.NugetHelpers;
public interface INugetPackageMirror
{
    Task<bool> MirrorLocalFeedAsync(NuGetPackageModel package, string nugetFile,
        CancellationToken cancellationToken = default); //this means there can even be a private version of this.
}
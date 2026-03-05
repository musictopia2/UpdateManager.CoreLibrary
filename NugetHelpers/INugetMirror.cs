namespace UpdateManager.CoreLibrary.NugetHelpers;
public interface INugetMirror
{
    Task<bool> MirrorLocalFeedAsync(EnumFeedType feedCategory, bool development, string nugetFile, CancellationToken cancellationToken = default); //this means there can even be a private version of this.
}
namespace UpdateManager.CoreLibrary.NugetHelpers;
public interface INugetPacker
{
    Task<bool> CreateNugetPackageAsync(INugetModel project, bool noBuild, CancellationToken cancellationToken = default);
}
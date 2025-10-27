namespace UpdateManager.CoreLibrary.NugetHelpers;
public interface INugetTemplatePacker
{
    //this time attempting to make sure it uses the actual nuget template model.   if i am later wrong, then create another interface.
    Task<bool> CreateNugetTemplatePackageAsync(NuGetTemplateModel template, CancellationToken cancellationToken = default);
}
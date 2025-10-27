namespace UpdateManager.CoreLibrary.Interfaces;
public interface ITemplatesContext
{
    Task<BasicList<NuGetTemplateModel>> GetTemplatesAsync();
    Task UpdateTemplateVersionAsync(string templateName, string version);
    Task UpdateTemplateAsync(NuGetTemplateModel updatedTemplate);
    Task AddTemplateAsync(NuGetTemplateModel template);
    Task SaveCompleteListAsync(BasicList<NuGetTemplateModel> templates);
}
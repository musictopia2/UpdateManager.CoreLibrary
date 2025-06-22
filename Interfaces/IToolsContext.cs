namespace UpdateManager.CoreLibrary.Interfaces;
public interface IToolsContext
{
    Task<BasicList<NuGetToolModel>> GetToolsAsync();
    Task UpdateToolVersionAsync(string toolName, string version);
    Task UpdateToolAsync(NuGetToolModel updatedTool);
    Task AddToolAsync(NuGetToolModel tool);
    Task SaveCompleteListAsync(BasicList<NuGetToolModel> tools);
}
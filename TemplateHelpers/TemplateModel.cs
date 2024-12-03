namespace UpdateManager.CoreLibrary.TemplateHelpers;
public class TemplateModel
{
    public string TemplateDirectory { get; set; } = "";
    public string CsProjFile { get; set; } = "";
    public bool Processed { get; set; }
}
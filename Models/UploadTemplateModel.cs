namespace UpdateManager.CoreLibrary.Models;
public class UploadTemplateModel
{
    public string PackageId { get; set; } = "";
    public string Version { get; set; } = ""; //this is needed to check if the version is available
    public string NugetFilePath { get; set; } = "";
    public bool Uploaded { get; set; }
}
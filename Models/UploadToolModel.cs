namespace UpdateManager.CoreLibrary.Models;
public class UploadToolModel
{
    public string PackageId { get; set; } = "";
    public string Version { get; set; } = ""; //this is needed to check if the version is available
    public string NugetFilePath { get; set; } = "";
    public bool Uploaded { get; set; } //at first, won't be uploaded.  if uploaded, then has to check to see if its there.
}
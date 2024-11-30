namespace UpdateManager.CoreLibrary.GeneralHelpers.Models;
public interface INugetModel
{
    string CsProjPath { get; set; }
    string NugetPackagePath { get; set; }
    string Version { get; set; }
}
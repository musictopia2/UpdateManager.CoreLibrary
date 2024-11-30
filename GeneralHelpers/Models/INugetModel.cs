namespace UpdateManager.CoreLibrary.GeneralHelpers.Models;
public interface INugetModel
{
    string CsProjPath { get; set; }
    string NugetPackagePath { get; set; }
    string Version { get; set; }
    bool Development { get; set; } //i am guessing some process may use this at some point (to decide what to do about it).
}
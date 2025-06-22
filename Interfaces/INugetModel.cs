namespace UpdateManager.CoreLibrary.Interfaces;
public interface INugetModel : IVersionable
{
    string CsProjPath { get; set; }
    string NugetPackagePath { get; set; }
    bool Development { get; set; } //i am guessing some process may use this at some point (to decide what to do about it).
}
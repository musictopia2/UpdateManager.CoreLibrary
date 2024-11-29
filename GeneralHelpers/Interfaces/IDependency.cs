namespace UpdateManager.CoreLibrary.GeneralHelpers.Interfaces;
public interface IDependency
{
    string PackageName { get; set; } // Allows setting the package name
    string Version { get; set; }     // Allows setting the version
}
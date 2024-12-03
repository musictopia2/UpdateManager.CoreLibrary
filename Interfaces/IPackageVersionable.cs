namespace UpdateManager.CoreLibrary.Interfaces;
public interface IPackageVersionable
{
    string PackageName { get; set; } // Allows setting the package name
    string Version { get; set; }     // Allows setting the version
}
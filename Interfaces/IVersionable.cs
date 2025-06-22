namespace UpdateManager.CoreLibrary.Interfaces;
public interface IVersionable
{
    /// <summary>
    /// The NuGet package ID. For both libraries and tools, this is the identifier used by NuGet.
    /// </summary>
    string PackageName { get; set; } // Allows setting the package name
    string PrefixForPackageName { get; set; }
    string Version { get; set; }     // Allows setting the version
}
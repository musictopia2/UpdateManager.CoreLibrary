namespace UpdateManager.CoreLibrary.Models;
public interface ILibraryUpgradeModel : INugetModel
{
    BasicList<string> Dependencies { get; set; }
}

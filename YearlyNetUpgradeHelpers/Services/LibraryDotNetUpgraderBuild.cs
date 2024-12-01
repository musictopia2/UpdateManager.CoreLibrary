using UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Extensions;
using UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Interfaces; //uncommon.  do here so i don't see it everywhere.
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class LibraryDotNetUpgraderBuild : ILibraryDotNetUpgraderBuild
{
    async Task<bool> ILibraryDotNetUpgraderBuild.BuildLibraryAsync(LibraryNetUpdateModel libraryModel, DotNetVersionUpgradeModel dotNetModel, BasicList<LibraryNetUpdateModel> libraries, CancellationToken cancellationToken)
    {
        CsProjEditor editor = new(libraryModel.CsProjPath);
        string netVersion = bb1.Configuration!.GetNetVersion();
        bool rets;
        rets = editor.UpdateNetVersion(netVersion);
        if (rets == false)
        {
            return false;
        }
        rets = await editor.UpdateDependenciesAsync(libraries);
        if (rets == false)
        {
            return false;
        }
        rets = await ProjectBuilder.BuildProjectAsync(libraryModel.CsProjPath, "/p:SkipPostBuild=true", cancellationToken);
        return rets;
    }
    async Task<bool> ILibraryDotNetUpgraderBuild.AlreadyUpgradedAsync(LibraryNetUpdateModel upgradeModel, DotNetVersionUpgradeModel dotNetModel)
    {
        return await upgradeModel.AlreadyUpgradedAsync(dotNetModel);
        


        //return packageNetVersion == netVersion;
    }

    


    
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class LibraryDotNetUpgraderBuild : ILibraryDotNetUpgraderBuild
{
    async Task<bool> ILibraryDotNetUpgraderBuild.BuildLibraryAsync(LibraryNetUpdateModel libraryModel, DotNetVersionUpgradeModel dotNetModel, BasicList<LibraryNetUpdateModel> libraries)
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

    }

    async Task<bool> ILibraryDotNetUpgraderBuild.AlreadyUpgradedAsync(LibraryNetUpdateModel upgradeModel, DotNetVersionUpgradeModel dotNetModel)
    {
        if (dotNetModel.IsTestMode)
        {
            return false; //since its testing, go ahead and go through the process of upgrading no matter what since its only testing anyways
        }
        string netVersion = bb1.Configuration!.GetNetVersion();
        if (upgradeModel.PackageType == EnumFeedType.Public)
        {
            //has to check something like 9.0.1.  if there on public nuget, then already done period.
            string upgradeVersion = $"{netVersion}.1.1";
            bool rets;
            rets = await NuGetPackageChecker.IsPackageAvailableAsync(upgradeModel.PackageName, upgradeVersion);
            return rets;
        }
        //hopefully there is a way with private packages to know if its already there.
        //the upgrdaeModel.Version showed the incremented amount.  most likely, the previous version would be there
        //however, not sure if there is a way to know what version of the .net it uses.
        return false;
    }
}
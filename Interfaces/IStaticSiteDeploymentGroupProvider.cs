namespace UpdateManager.CoreLibrary.Interfaces;
public interface IStaticSiteDeploymentGroupProvider
{
    Task<BasicList<StaticSiteDeploymentGroupModel>>
        GetDeploymentGroupsAsync();
}
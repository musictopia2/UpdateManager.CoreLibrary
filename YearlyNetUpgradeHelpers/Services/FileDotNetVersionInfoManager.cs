namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class FileDotNetVersionInfoManager : IDotNetVersionInfoManager
{
    private static readonly string _netPath = bb1.Configuration!.GetNetPath();
    Task IDotNetVersionInfoManager.UpdateVersionAsync(DotNetVersionUpgradeModel model)
    {
        string text = GetContents(model);
        return ff1.WriteAllTextAsync(_netPath, text);
    }
    private static string GetContents(DotNetVersionUpgradeModel config)
    {
        var sb = new StringBuilder();
        // Manually append the properties and their values
        sb.AppendLine($"NetVersion\t{config.NetVersion}");
        sb.AppendLine($"LastUpdated\t{config.LastUpdated:MM/dd/yyyy}");  // Format the DateOnly value
        sb.AppendLine($"IsTestMode\t{config.IsTestMode}");
        sb.AppendLine($"TestLocalFeedPath\t{config.TestLocalFeedPath}");
        sb.AppendLine($"TestPublicFeedPath\t{config.TestPublicFeedPath}");
        //no longer necessary to have a temporary feed for production because i have staging.  they will go to staging.
        //sb.AppendLine($"ProdPrivateFeedPath\t{config.ProdPrivateFeedPath}");
        //if you want specific stuff, has to go somewhere else now.
        //sb.AppendLine($"NeedsGamePackageCommit\t{config.NeedsGamePackageCommit}");
        //sb.AppendLine($"NeedsTemplatesUpgraded\t{config.NeedsTemplatesUpgraded}");
        return sb.ToString();
    }
}
namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.Services;
public class FileDotNetVersionInfoManager : IDotNetVersionInfoRepository
{
    private static readonly string _netPath = bb1.Configuration!.GetNetPath();
    public async Task SaveVersionInfoAsync(DotNetVersionUpgradeModel model)
    {
        // Step 1: Get the old data from the file before saving
        string oldFileContent = await ff1.AllTextAsync(_netPath);
        // Step 2: Save the new data to the file
        string newFileContent = GetContents(model);
        await ff1.WriteAllTextAsync(_netPath, newFileContent);
        // Step 3: Ensure the file saved properly and matches with IConfiguration
        bool success = VerifyFileContent(model);
        if (!success)
        {
            // If the verification fails, revert to the old file content
            await ff1.WriteAllTextAsync(_netPath, oldFileContent);

            // Raise an exception after rolling back
            throw new InvalidOperationException(
                "Version info was not correctly updated. The changes were rolled back. " +
                "If using the 'AddTextFile' extension to load the configuration, ensure that the 'ReloadOnChange' parameter is set to 'true'. " +
                "This ensures that changes to the configuration file are reflected in the application dynamically. " +
                "Example: builder.AddTextFile('yourpath', false, true);");

        }
    }
    private static string GetContents(DotNetVersionUpgradeModel config)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"NetVersion\t{config.NetVersion}");
        sb.AppendLine($"LastUpdated\t{config.LastUpdated:MM/dd/yyyy}");  // Format the DateOnly value
        sb.AppendLine($"IsTestMode\t{config.IsTestMode}");
        sb.AppendLine($"TestLocalFeedPath\t{config.TestLocalFeedPath}");
        sb.AppendLine($"TestPublicFeedPath\t{config.TestPublicFeedPath}");

        // Return the constructed text content
        return sb.ToString();
    }
    private static bool VerifyFileContent(DotNetVersionUpgradeModel expectedModel)
    {
        // Retrieve the version from IConfiguration after it has been reloaded
        string versionFoundString = bb1.Configuration!.GetNetVersion();

        // Attempt to parse the NetVersion from IConfiguration
        bool rets = int.TryParse(versionFoundString, out int versionFoundInt);
        if (!rets)
        {
            return false; // If the NetVersion is not an integer, it's invalid
        }

        // Verify that the NetVersion matches the expected value
        return versionFoundInt == expectedModel.NetVersion;
    }

    private static DotNetVersionUpgradeModel ParseFileContent(string content)
    {
        var model = new DotNetVersionUpgradeModel();
        var lines = content.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('\t');
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                switch (key)
                {
                    case "NetVersion":
                        model.NetVersion = int.Parse(value);
                        break;
                    case "LastUpdated":
                        model.LastUpdated = DateOnly.Parse(value);
                        break;
                    case "IsTestMode":
                        model.IsTestMode = bool.Parse(value);
                        break;
                    case "TestLocalFeedPath":
                        model.TestLocalFeedPath = value;
                        break;
                    case "TestPublicFeedPath":
                        model.TestPublicFeedPath = value;
                        break;
                    default:
                        // Log or handle unknown keys if necessary
                        break;
                }
            }
        }
        return model;
    }

    public async Task<DotNetVersionUpgradeModel> GetVersionInfoAsync()
    {
        // Read the content from the file
        string fileContent = await ff1.AllTextAsync(_netPath);

        // Parse the file content and return the model
        var model = ParseFileContent(fileContent);
        return model;
    }
}
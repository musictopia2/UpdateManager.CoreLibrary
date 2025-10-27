namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetToolManager
{
    // Install globally, version optional (null means latest)
    public static async Task InstallToolAsync(string toolId, string? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolId))
        {
            throw new ArgumentException("Tool ID must be provided.", nameof(toolId));
        }
        string versionArg = string.IsNullOrEmpty(version) ? "" : $" --version {version}";
        string arguments = $"tool install -g {toolId}{versionArg}";
        await NuGetGeneralManager.RunDotnetCommandAsync(arguments, cancellationToken);
    }

    // Uninstall globally, version optional (null means uninstall all versions)
    public static async Task UninstallToolAsync(string toolId, string? version = null, CancellationToken cancellationToken = default)
    {
        string versionArg = string.IsNullOrEmpty(version) ? "" : $" --version {version}";
        string arguments = $"tool uninstall -g {toolId}{versionArg}";
        try
        {
            await NuGetGeneralManager.RunDotnetCommandAsync(arguments, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("is not currently installed", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("could not be found", StringComparison.OrdinalIgnoreCase))
            {
                // Tool is not installed — this is fine
                return;
            }

            // Re-throw for all other cases
            throw;
        }
    }

    // Helper method to run dotnet CLI commands
    

}

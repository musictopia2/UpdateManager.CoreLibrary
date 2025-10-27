namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetTemplateManager
{
    public static async Task InstallTemplateAsync(string templateId, string? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new ArgumentException("Template ID must be provided.", nameof(templateId));
        }
        string versionArg = string.IsNullOrEmpty(version) ? "" : $"::{version}";
        string arguments = $"new install {templateId}{versionArg}";
        await NuGetGeneralManager.RunDotnetCommandAsync(arguments, cancellationToken);
    }
    public static async Task UninstallTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        string arguments = $"new uninstall {templateId}";
        try
        {
            await NuGetGeneralManager.RunDotnetCommandAsync(arguments, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("is not found", StringComparison.OrdinalIgnoreCase))
            {
                // Template is not installed — this is fine
                return;
            }

            // Re-throw for all other cases
            throw;
        }
    }
}

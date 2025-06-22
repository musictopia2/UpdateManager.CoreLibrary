namespace UpdateManager.CoreLibrary.NugetHelpers;
public class NuGetToolManager
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
        await RunDotnetCommandAsync(arguments, cancellationToken);
    }

    // Uninstall globally, version optional (null means uninstall all versions)
    public static async Task UninstallToolAsync(string toolId, string? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolId))
        {
            throw new ArgumentException("Tool ID must be provided.", nameof(toolId));
        }
        string versionArg = string.IsNullOrEmpty(version) ? "" : $" --version {version}";
        string arguments = $"tool uninstall -g {toolId}{versionArg}";
        await RunDotnetCommandAsync(arguments, cancellationToken);
    }

    // Helper method to run dotnet CLI commands
    private static async Task RunDotnetCommandAsync(string arguments, CancellationToken token = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync(token);
        string error = await process.StandardError.ReadToEndAsync(token);

        await process.WaitForExitAsync(token);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet command failed: {error}");
        }
    }

}

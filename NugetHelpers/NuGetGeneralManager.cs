namespace UpdateManager.CoreLibrary.NugetHelpers;
internal static class NuGetGeneralManager
{
    public static async Task RunDotnetCommandAsync(string arguments, CancellationToken token = default)
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

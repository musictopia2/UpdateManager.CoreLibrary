using System.Text.Json;

namespace UpdateManager.CoreLibrary.Utilities;

public static class ProjectPublisher
{
    public static async Task<bool> PublishProjectWindowsSelfContainedAsync(
        string csprojFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            string workingDirectory = Path.GetDirectoryName(csprojFilePath)!;
            string[] projectProperties =
            [
                "-p:Configuration=Release",
                "-p:RuntimeIdentifier=win-x64",
                "-p:SelfContained=true",
                "-p:SkipAppPostBuild=true"
            ];

            var (evaluationExitCode, propertiesJson, evaluationErrors) = await RunDotNetAsync(
                workingDirectory,
                ["msbuild", csprojFilePath, "-nologo", .. projectProperties,
                    "-getProperty:UseWPF,TargetFrameworkVersion"],
                cancellationToken);

            if (evaluationExitCode != 0)
            {
                Console.WriteLine($"Could not determine the publish format:{Environment.NewLine}{propertiesJson}{evaluationErrors}");
                return false;
            }

            using var evaluation = JsonDocument.Parse(propertiesJson);
            JsonElement properties = evaluation.RootElement.GetProperty("Properties");

            // WPF mixed-mode assemblies can fail to load from a .NET 11 single-file bundle.
            // https://github.com/dotnet/wpf/issues/11842
            bool useFolderPublish =
                string.Equals(properties.GetProperty("UseWPF").GetString(), "true",
                    StringComparison.OrdinalIgnoreCase) &&
                properties.GetProperty("TargetFrameworkVersion").GetString() == "v11.0";

            string publishMode = useFolderPublish ? "folder" : "single file";
            Console.WriteLine($"Publish mode: self-contained {publishMode} (win-x64).");

            var (exitCode, output, errors) = await RunDotNetAsync(
                workingDirectory,
                ["publish", csprojFilePath, .. projectProperties, "--output", "bin/Release/Publish",
                    useFolderPublish ? "-p:PublishSingleFile=false" : "-p:PublishSingleFile=true"],
                cancellationToken);

            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine($"Publish Errors: {errors}");
            }

            if (exitCode != 0)
            {
                Console.WriteLine($"Publish output: {output}");
            }

            return exitCode == 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Publish process was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while publishing the project: {ex.Message}");
            return false;
        }
    }

    private static async Task<(int ExitCode, string Output, string Errors)> RunDotNetAsync(
        string workingDirectory, string[] arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start dotnet.");

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            return (process.ExitCode, await outputTask, await errorTask);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (process.HasExited)
                {
                }
            }

            throw;
        }
    }
}

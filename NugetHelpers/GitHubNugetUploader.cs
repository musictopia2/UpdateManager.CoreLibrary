using System.Text.Json;

namespace UpdateManager.CoreLibrary.NugetHelpers;

public sealed class GitHubNugetUploader(IGitHubNugetConfiguration config) : INugetUploader
{
    private readonly IGitHubNugetConfiguration _config = config ??
            throw new ArgumentNullException(nameof(config));

    public async Task<bool> UploadNugetPackageAsync(
        string nugetFilePath,
        string packageId,
        string packageVersion,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(nugetFilePath))
        {
            Console.WriteLine("NuGet package file not found.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(packageId) ||
            string.IsNullOrWhiteSpace(packageVersion))
        {
            Console.WriteLine("Package ID and version are required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_config.Repository) ||
            string.IsNullOrWhiteSpace(_config.Workflow) ||
            _config.Timeout <= TimeSpan.Zero)
        {
            Console.WriteLine("Invalid GitHub NuGet configuration.");
            return false;
        }

        try
        {
            using CancellationTokenSource timeoutSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutSource.CancelAfter(_config.Timeout);

            CancellationToken token = timeoutSource.Token;

            string packageName = packageId.ToLowerInvariant();

            string releaseTag =
                $"staging-{packageName}-{packageVersion}";

            string expectedFile =
                $"{packageName}.{packageVersion}.nupkg";

            string actualFile = Path.GetFileName(nugetFilePath);

            if (!string.Equals(
                actualFile,
                expectedFile,
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"Expected package filename: {expectedFile}");
                return false;
            }

            string requestId = Guid.NewGuid().ToString("N");

            Console.WriteLine(
                $"Preparing GitHub release: {releaseTag}");

            var release = await RunGhAsync(
                token,
                "release", "view", releaseTag,
                "--repo", _config.Repository);

            if (!release.Success)
            {
                var create = await RunGhAsync(
                    token,
                    "release", "create", releaseTag,
                    "--repo", _config.Repository,
                    "--title",
                    $"NuGet staging {packageId} {packageVersion}",
                    "--notes",
                    "Package staging for NuGet publishing.",
                    "--draft",
                    "--latest=false");

                if (!create.Success)
                {
                    LogFailure(create);
                    return false;
                }
            }

            Console.WriteLine($"Uploading package: {actualFile}");

            var upload = await RunGhAsync(
                token,
                "release", "upload", releaseTag,
                nugetFilePath,
                "--repo", _config.Repository,
                "--clobber");

            if (!upload.Success)
            {
                LogFailure(upload);
                return false;
            }

            // GitHub Actions must be able to download the release.
            var publishRelease = await RunGhAsync(
                token,
                "release", "edit", releaseTag,
                "--repo", _config.Repository,
                "--draft=false");

            if (!publishRelease.Success)
            {
                LogFailure(publishRelease);
                return false;
            }

            DateTimeOffset dispatchStarted =
                DateTimeOffset.UtcNow.AddSeconds(-10);

            Console.WriteLine("Starting GitHub Actions.");

            var dispatch = await RunGhAsync(
                token,
                "workflow", "run", _config.Workflow,
                "--repo", _config.Repository,
                "-f", $"package_id={packageId}",
                "-f", $"package_version={packageVersion}",
                "-f", $"request_id={requestId}");

            if (!dispatch.Success)
            {
                LogFailure(dispatch);
                return false;
            }

            Console.WriteLine(
                $"Waiting for publishing request {requestId}");

            long? runId = null;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (runId is null)
                {
                    var runs = await RunGhAsync(
                        token,
                        "run", "list",
                        "--repo", _config.Repository,
                        "--workflow", _config.Workflow,
                        "--event", "workflow_dispatch",
                        "--limit", "100",
                        "--json",
                        "databaseId,displayTitle,createdAt");

                    if (!runs.Success)
                    {
                        LogFailure(runs);
                        return false;
                    }

                    using JsonDocument document =
                        JsonDocument.Parse(runs.Output);

                    foreach (JsonElement item in
                        document.RootElement.EnumerateArray())
                    {
                        string title =
                            item.GetProperty("displayTitle")
                                .GetString() ?? "";

                        DateTimeOffset created =
                            item.GetProperty("createdAt")
                                .GetDateTimeOffset();

                        if (created < dispatchStarted)
                        {
                            continue;
                        }

                        if (title.EndsWith(
                            requestId,
                            StringComparison.Ordinal))
                        {
                            runId = item.GetProperty("databaseId")
                                .GetInt64();

                            Console.WriteLine(
                                $"Found GitHub Actions run: {runId}");

                            break;
                        }
                    }
                }
                else
                {
                    var result = await RunGhAsync(
                        token,
                        "run", "view", runId.Value.ToString(),
                        "--repo", _config.Repository,
                        "--json", "status,conclusion");

                    if (!result.Success)
                    {
                        LogFailure(result);
                        return false;
                    }

                    using JsonDocument document =
                        JsonDocument.Parse(result.Output);

                    JsonElement root = document.RootElement;

                    string status =
                        root.GetProperty("status").GetString() ?? "";

                    string conclusion =
                        root.GetProperty("conclusion").GetString() ?? "";

                    if (status == "completed")
                    {
                        if (conclusion == "success")
                        {
                            Console.WriteLine(
                                "GitHub NuGet publishing succeeded.");
                            return true;
                        }

                        Console.WriteLine(
                            $"GitHub publishing failed: {conclusion}");

                        return false;
                    }
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5), token);
            }
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("GitHub publishing timed out.");
            return false;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"GitHub publishing error: {ex.Message}");
            return false;
        }
    }

    private static async Task<(
        bool Success,
        string Output,
        string Error)> RunGhAsync(
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        ProcessStartInfo info = new()
        {
            FileName = "gh",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        using Process process = new()
        {
            StartInfo = info
        };

        process.Start();

        Task<string> outputTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        Task<string> errorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);

            string output = await outputTask;
            string error = await errorTask;

            return (
                process.ExitCode == 0,
                output,
                error);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw;
        }
    }

    private static void LogFailure(
        (bool Success, string Output, string Error) result)
    {
        Console.WriteLine(
            string.IsNullOrWhiteSpace(result.Error)
                ? result.Output
                : result.Error);
    }
}

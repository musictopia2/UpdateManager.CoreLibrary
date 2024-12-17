namespace UpdateManager.CoreLibrary.Utilities;
public static class GitHubCommitter
{
    private static bool _isCommitInProgress = false;
    public static async Task<bool> CommitAndPushToGitHubAsync(string csprojFilePath, string commitMessage, CancellationToken cancellationToken = default)
    {
        if (_isCommitInProgress)
        {
            Console.WriteLine("Commit operation is already in progress.");
            return false; // Or throw an exception, depending on your preference
        }
        try
        {
            _isCommitInProgress = true;
            if (string.IsNullOrWhiteSpace(csprojFilePath) || !csprojFilePath.EndsWith(".csproj"))
            {
                Console.WriteLine("Error: Invalid .csproj file path.");
                return false;
            }
            string projectDir = Path.GetDirectoryName(csprojFilePath)!;
            if (IsGitRepository(projectDir) == false)
            {
                return true; //there is no repository so its okay.
            }
            return await PrivateCommitAsync(projectDir, commitMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Git operations: {ex.Message}");
            return false;
        }
        finally
        {
            // Reset flag after commit is complete
            _isCommitInProgress = false;
        }
    }
    private static async Task<bool> PrivateCommitAsync(string directoryPath, string commitMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                Console.WriteLine("Error: Invalid directory path.");
                return false;
            }

            if (!IsGitRepository(directoryPath))
            {
                Console.WriteLine("Error: The specified directory is not a Git repository.");
                return false;
            }

            // Check if there are uncommitted changes
            var uncommittedChanges = await HasUncommittedChangesAsync(directoryPath, cancellationToken);

            if (!uncommittedChanges)
            {
                return true; // Nothing to commit, so return true
            }

            var (IsSuccess, Output, ErrorMessage) = await RunGitCommandAsync(directoryPath, "add .", cancellationToken);

            if (!IsSuccess)
            {
                Console.WriteLine($"Error during git add: {ErrorMessage}");
                return false;
            }

            var commitResult = await RunGitCommandAsync(directoryPath, $"commit -m \"{commitMessage}\"", cancellationToken);

            if (!commitResult.IsSuccess)
            {
                Console.WriteLine($"Error during git commit: {commitResult.ErrorMessage}");
                return false;
            }

            var pushResult = await RunGitCommandAsync(directoryPath, "push origin", cancellationToken);

            if (!pushResult.IsSuccess)
            {
                Console.WriteLine($"Error during git push: {pushResult.ErrorMessage}");
                return false;
            }

            Console.WriteLine("Changes committed and pushed successfully.");
            return true; // Indicate success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Git operations: {ex.Message}");
            return false; // Indicate failure
        }
    }
    public static async Task<bool> CommitDirectoryToGitHubAsync(string directoryPath, string commitMessage, CancellationToken cancellationToken = default)
    {
        if (_isCommitInProgress)
        {
            Console.WriteLine("Commit operation is already in progress.");
            return false; // Or throw an exception, depending on your preference
        }
        _isCommitInProgress = true;
        bool rets = await PrivateCommitAsync(directoryPath, commitMessage, cancellationToken);
        _isCommitInProgress = false;
        return rets;
    }

    // Helper function to check if there are uncommitted changes
    public static async Task<bool> HasUncommittedChangesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var (_, Output, _) = await RunGitCommandAsync(directoryPath, "status --porcelain", cancellationToken);

        // If the output is empty, there are no changes
        return !string.IsNullOrEmpty(Output);
    }

    // Helper function to run a Git command asynchronously
    static async Task<(bool IsSuccess, string Output, string ErrorMessage)> RunGitCommandAsync(string workingDirectory, string command, CancellationToken cancellationToken = default)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(processInfo) ?? throw new InvalidOperationException("Failed to start git process.");
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        bool isSuccess = process.ExitCode == 0;

        if (!string.IsNullOrEmpty(error) && isSuccess)
        {
            Console.WriteLine($"Git output: {output}");
            return (true, output, error);
        }

        if (!isSuccess)
        {
            Console.WriteLine($"Git command failed: {error}");
        }

        return (isSuccess, output, error);
    }

    // Helper function to check if the directory is a Git repository
    public static bool IsGitRepository(string directory)
    {
        if (Directory.Exists(Path.Combine(directory, ".git")))
        {
            return true;
        }
        // Only check one level up for this version
        directory = Path.GetDirectoryName(directory)!;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }
        return Directory.Exists(Path.Combine(directory, ".git"));
    }
}
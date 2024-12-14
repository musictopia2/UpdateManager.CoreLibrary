namespace UpdateManager.CoreLibrary.Utilities;
public static class GitBranchManager
{

    private static async Task<string> GetDefaultBranchAsync(string repoDirectory, CancellationToken cancellationToken = default)
    {
        // Run the command to get a list of branches, with the default branch marked
        var (isSuccess, output, errorMessage) = await RunGitCommandAsync(repoDirectory, "branch -r", cancellationToken);

        if (!isSuccess)
        {
            Console.WriteLine($"Error getting branches: {errorMessage}");
            return "";
        }

        // Check if 'main' or 'master' exists in the remote branches
        if (output.Contains("origin/main", StringComparison.CurrentCultureIgnoreCase))
        {
            return "main";
        }
        else if (output.Contains("origin/master", StringComparison.CurrentCultureIgnoreCase))
        {
            return "master";
        }

        Console.WriteLine($"Neither 'main' nor 'master' branches found. What was found was {output}");
        return "";
    }


    private static bool _isBranchSwitchInProgress = false;

    // Switches to the specified branch before performing updates
    public static async Task<bool> SwitchBranchToDefaultAsync(string repoDirectory, CancellationToken cancellationToken = default)
    {
        if (_isBranchSwitchInProgress)
        {
            Console.WriteLine("Branch switch operation is already in progress.");
            return false;
        }

        try
        {
            _isBranchSwitchInProgress = true;

            if (IsGitRepository(repoDirectory) == false)
            {
                return true; //go ahead and return true because you don't even have a repository so no problem.
            }
            string targetBranch = await GetDefaultBranchAsync(repoDirectory, cancellationToken);
            if (targetBranch == "")
            {
                return false;
            }
            // Check the current branch
            var currentBranch = await GetCurrentBranchAsync(repoDirectory, cancellationToken);
            if (currentBranch != targetBranch)
            {

                // Ensure no uncommitted changes before switching branches if you are not on the proper branch.
                if (await HasUncommittedChangesAsync(repoDirectory, cancellationToken))
                {
                    Console.WriteLine("There are uncommitted changes. Please commit or stash your changes before switching branches.");
                    return false;
                }


                //Console.WriteLine($"Switching from branch {currentBranch} to {targetBranch}...");
                var (IsSuccess, Output, ErrorMessage) = await RunGitCommandAsync(repoDirectory, $"checkout {targetBranch}", cancellationToken);

                if (!IsSuccess)
                {
                    Console.WriteLine($"Error during git checkout: {ErrorMessage}");
                    return false;
                }

                //Console.WriteLine($"Successfully switched to branch {targetBranch}.");
            }
            //else
            //{
            //    Console.WriteLine($"Already on the target branch: {targetBranch}.");
            //}

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during branch switch operation: {ex.Message}");
            return false;
        }
        finally
        {
            _isBranchSwitchInProgress = false;
        }
    }

    // Get the current branch name
    public static async Task<string> GetCurrentBranchAsync(string repoDirectory, CancellationToken cancellationToken = default)
    {
        var (isSuccess, output, errorMessage) = await RunGitCommandAsync(repoDirectory, "rev-parse --abbrev-ref HEAD", cancellationToken);

        if (!isSuccess)
        {
            Console.WriteLine($"Error getting current branch: {errorMessage}");
            return string.Empty;
        }

        return output.Trim();
    }

    // Helper function to check if there are uncommitted changes
    public static async Task<bool> HasUncommittedChangesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var (_, Output, _) = await RunGitCommandAsync(directoryPath, "status --porcelain", cancellationToken);

        // If the output is empty, there are no changes
        return !string.IsNullOrEmpty(Output);
    }

    // Helper function to run a Git command asynchronously
    private static async Task<(bool IsSuccess, string Output, string ErrorMessage)> RunGitCommandAsync(string workingDirectory, string command, CancellationToken cancellationToken = default)
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
            //Console.WriteLine($"Git output: {output}");
            return (true, output, error);
        }

        //if (!isSuccess)
        //{
        //    Console.WriteLine($"Git command failed: {error}");
        //}

        return (isSuccess, output, error);
    }
    // Helper function to check if the directory is a Git repository
    private static bool IsGitRepository(string directory)
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
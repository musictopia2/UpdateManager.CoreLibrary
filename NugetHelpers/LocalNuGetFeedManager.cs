namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class LocalNuGetFeedManager
{
    // Check if the package exists in the feed
    public static async Task<bool> PackageExistsAsync(string feedUrl, string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget",
                Arguments = $"list {packageName} -source {feedUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            var exitTask = process.WaitForExitAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask, exitTask);

            return process.ExitCode == 0; // 0 indicates the package exists
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking package: {ex.Message}");
            return false;
        }
    }

    // Delete all versions of a package from the feed by deleting the package folder
    public static async Task<bool> DeletePackageFolderAsync(string feedPath, string packageName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(feedPath) || string.IsNullOrEmpty(packageName))
            {
                Console.WriteLine("Feed path and package name must be provided.");
                Environment.Exit(1); //this is an error.
                return false;
            }

            // Step 1: Identify the full package folder path
            var packageFolderPath = Path.Combine(feedPath, packageName);

            // Check if the folder exists
            if (ff1.DirectoryExists(packageFolderPath) == false)
            {
                //if this needs to be notified, something else should do it.
                return false;
            }

            // Step 2: Delete the entire package folder (including versions and metadata)
            try
            {
                await ff1.DeleteFolderAsync(packageFolderPath);
                //Directory.Delete(packageFolderPath, true);  // 'true' to delete all subdirectories and files
                Console.WriteLine($"Package folder {packageFolderPath} deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting package folder {packageFolderPath}: {ex.Message}");
                Environment.Exit(1);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting package folder: {ex.Message}");
            Environment.Exit(1);
            return false;
        }
    }

    // Get all package names in the feed (without versions)
    public static async Task<IEnumerable<string>> GetAllPackagesAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget",
                Arguments = $"list -source {feedUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            var exitTask = process.WaitForExitAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask, exitTask);

            if (process.ExitCode == 0)
            {
                // Parse the output to extract just the package names (ignore versions)
                var packageList = (await outputTask).Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

                // Extract the package names (the first part before the version number)
                var packageNames = packageList
                    .Select(pkg => pkg.Trim().Split(' ')[0])  // Take only the first part (package name)
                    .Distinct()                               // Remove duplicates, in case a package has multiple versions
                    .ToList();

                return packageNames;
            }
            else
            {
                Console.WriteLine($"Error fetching package names: {await errorTask}");
                return [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all package names: {ex.Message}");
            return [];
        }
    }
    // Get the latest version of a package in the feed
    public static async Task<string?> GetLatestPackageVersionAsync(string feedUrl, string packageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget",
                Arguments = $"list {packageId} -source {feedUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            var exitTask = process.WaitForExitAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask, exitTask);

            if (process.ExitCode == 0)
            {
                // Parse the output to extract the package versions
                var packageList = (await outputTask).Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

                // Extract versions and sort them
                var versions = packageList
                    .Select(pkg => pkg.Trim().Split(' '))  // Split by space
                    .Where(parts => parts.Length > 1)      // Ensure we have a package ID and version
                    .Select(parts => parts[1])             // Take the version part
                    .OrderByDescending(version => version) // Sort versions in descending order
                    .ToList();

                return versions.FirstOrDefault();  // Return the latest version
            }
            else
            {
                Console.WriteLine($"Error fetching package versions: {await errorTask}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting latest package version: {ex.Message}");
            return null;
        }
    }
}
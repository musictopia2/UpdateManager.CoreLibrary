namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class LocalNuGetFeedUploader
{
    public static async Task<bool> UploadPrivateNugetPackageAsync(string feedPath, string nugetPath, string nugetFile, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget",
                Arguments = $"add {nugetFile} -source {feedPath}",
                WorkingDirectory = nugetPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = startInfo };
            // Start the process asynchronously
            process.Start();

            // Read the output and error asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            // Wait for process exit with cancellation support
            var exitTask = process.WaitForExitAsync(cancellationToken);

            // Wait for all tasks to complete
            await Task.WhenAll(outputTask, errorTask, exitTask);

            // Check the process exit code
            if (process.ExitCode == 0)
            {
                Console.WriteLine("Package successfully uploaded.");
                return true;
            }
            else
            {
                Console.WriteLine($"Error publishing NuGet package: {await errorTask}");
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
            Console.WriteLine("Operation was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }
}
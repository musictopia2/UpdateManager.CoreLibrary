namespace UpdateManager.CoreLibrary.GeneralHelpers.Utilities;
public static class ProjectBuilder
{
    public static async Task<bool> BuildProjectAsync(string csprojFilePath, string extraArguments = "", CancellationToken cancellationToken = default)
    {
        try
        {
            string ends = string.IsNullOrEmpty(extraArguments) ? "" : $" {extraArguments}";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{csprojFilePath}\" -c Release{ends}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start the build process.");
                return false;
            }

            // Read and display the output and errors for diagnostics
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            // Wait for the process to exit and check for cancellation
            var exitTask = process.WaitForExitAsync(cancellationToken);
            await Task.WhenAny(outputTask, errorTask, exitTask); // Wait for any task to complete

            if (cancellationToken.IsCancellationRequested)
            {
                try
                {
                    process.Kill(); // Kill the process on cancellation
                }
                catch (Exception killEx)
                {
                    Console.WriteLine($"Error killing the process: {killEx.Message}");
                }

                Console.WriteLine("Build process was canceled.");
                return false;
            }
            string output = await outputTask;
            string errors = await errorTask;

            // Log errors if any
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine($"Build Errors: {errors}");
            }
            // Wait for the process to fully exit
            await exitTask;

            // Return true if the build process was successful, false otherwise
            bool success = process.ExitCode == 0;
            if (!success)
            {
                Console.WriteLine($"Build output: {output}");
            }
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while building the project: {ex.Message}");
            return false;
        }
    }
}
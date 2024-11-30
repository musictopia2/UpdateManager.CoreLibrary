namespace UpdateManager.CoreLibrary.GeneralHelpers.NugetHelpers;
public class PublicNugetUploader : INugetUploader
{
    private static IConfiguration Configuration => bb1.Configuration!;

    // Async method to upload the NuGet package with cancellation support
    public async Task<bool> UploadNugetPackageAsync(string nugetFilePath, CancellationToken cancellationToken = default)
    {
        // Make sure the provided file exists
        if (!File.Exists(nugetFilePath))
        {
            Console.WriteLine("Error: NuGet package file not found.");
            return false;
        }

        // Output for logging the upload attempt
        Console.WriteLine($"Uploading Package {nugetFilePath}");

        string key = Configuration.GetValue<string>(NugetConstants.NugetKey)!;

        // Extract the directory of the nuget package file to set as the working directory
        string workingDirectory = Path.GetDirectoryName(nugetFilePath)!;

        // Setup process to call dotnet nuget push
        ProcessStartInfo psi = new()
        {
            WorkingDirectory = workingDirectory,  // Use directory of .nupkg file
            FileName = "dotnet",
            Arguments = $"nuget push \"{nugetFilePath}\" -k {key} -s https://api.nuget.org/v3/index.json",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        try
        {
            // Start process asynchronously
            using Process process = new();
            process.StartInfo = psi;
            process.Start();

            // Read output and error streams asynchronously
            var (isSuccess, output, error) = await RunProcessAsync(process, cancellationToken);

            // If process fails, print the error message
            if (!isSuccess)
            {
                if (output.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Error: Package version already exists.");
                }
                else
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
            return isSuccess;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            return false;
        }
    }

    // Helper method to run a process and capture output and error asynchronously
    private static async Task<(bool IsSuccess, string Output, string ErrorMessage)> RunProcessAsync(Process process, CancellationToken cancellationToken)
    {
        // Read output and error asynchronously
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);

        // Wait for the process to exit with cancellation support
        await process.WaitForExitAsync(cancellationToken);

        bool isSuccess = process.ExitCode == 0;

        return (isSuccess, output, error);
    }
}
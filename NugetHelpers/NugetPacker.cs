namespace UpdateManager.CoreLibrary.NugetHelpers;
public class NugetPacker : INugetPacker
{
    public async Task<bool> CreateNugetPackageAsync(INugetModel project, bool noBuild, CancellationToken cancellationToken)
    {
        var projectDirectory = Path.GetDirectoryName(project.CsProjPath);
        if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
        {
            Console.WriteLine("Invalid or non-existent project directory.");
            return false;
        }
        Console.WriteLine($"Creating Package For {projectDirectory}");
        try
        {
            // Clear any existing .nupkg files before creating a new one
            await ff1.DeleteSeveralFilesAsync(project.NugetPackagePath, ".nupkg");

            string extras = "";
            if (noBuild)
            {
                extras = " --no-build ";  // This flag will skip the build step if specified
            }

            // Determine the correct PackageId (use PrefixForPackageName + PackageName if PrefixForPackageName is provided)
            string packageId = project.GetPackageID();
            
            // Construct the full command with the .csproj path, other arguments, and the custom PackageId if provided
            string arguments = $"pack \"{project.CsProjPath}\" -c release {extras} -p:PackageVersion={project.Version} -p:PackageId={packageId}";
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,  // Using the full path to the .csproj file
                WorkingDirectory = projectDirectory,  // Ensure we are in the correct directory
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);

            if (process == null)
            {
                Console.WriteLine("Failed to start the process.");
                return false;
            }

            // Capture output and errors from the process
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            // Check if the process was cancelled
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Process was cancelled.");
                process.Kill();
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);

            // Check for any errors based on the process exit code
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Error creating NuGet package: {error}");
                return false;
            }

            Console.WriteLine("NuGet package created successfully.");
            return true;
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the process
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }
}

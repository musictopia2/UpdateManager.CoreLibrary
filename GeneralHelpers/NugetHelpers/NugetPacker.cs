namespace UpdateManager.CoreLibrary.GeneralHelpers.NugetHelpers;
public class NugetPacker : INugetPacker
{
    async Task<bool> INugetPacker.CreateNugetPackageAsync(INugetModel project, bool noBuild, CancellationToken cancellationToken)
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
            await ff1.DeleteSeveralFilesAsync(project.NugetPackagePath, ".nupkg");
            string extras = "";
            if (noBuild)
            {
                extras = " --nobuild ";
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                // Specify the full path to the csproj file
                Arguments = $"pack \"{project.CsProjPath}\" -c release {extras} -p:PackageVersion={project.Version}",  // Using the full path to the csproj
                WorkingDirectory = projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            var output = await process!.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Process was cancelled.");
                process.Kill();
                return false;
            }
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Error creating NuGet package: {output}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return false;
        }
    }
}
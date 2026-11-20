namespace UpdateManager.CoreLibrary.Utilities;
public static class ProjectPublisher
{
    // .NET 11 WPF loads these mixed-mode dependencies from disk (dotnet/wpf#11842).
    private const string WpfPublishTargets = """
        <Project>
          <Target Name="KeepNet11WpfRuntimeFilesOutsideBundle"
                  AfterTargets="ComputeResolvedFilesToPublishList"
                  Condition="'$(UseWPF)' == 'true' And '$(TargetFrameworkVersion)' == 'v11.0'
                             And '$(PublishSingleFile)' == 'true' And '$(SelfContained)' == 'true'">
            <ItemGroup>
              <ResolvedFileToPublish Update="@(ResolvedFileToPublish)"
                                     Condition="'%(Filename)%(Extension)' == 'DirectWriteForwarder.dll'
                                             Or '%(Filename)%(Extension)' == 'System.Printing.dll'
                                             Or '%(Filename)%(Extension)' == 'ijwhost.dll'"
                                     ExcludeFromSingleFile="true" />
            </ItemGroup>
          </Target>
        </Project>
        """;

    public static async Task<bool> PublishProjectWindowsSelfContainedAsync(string csprojFilePath, CancellationToken cancellationToken = default)
    {
        string? targetsPath = null;
        try
        {
            targetsPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(targetsPath, WpfPublishTargets, cancellationToken);

            string workingDirectory = Path.GetDirectoryName(csprojFilePath)!;
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true
            };

            foreach (string argument in new[]
            {
                "publish", csprojFilePath, "--configuration", "Release",
                "--runtime", "win-x64", "--output", "bin/Release/Publish",
                "--self-contained", "true", "/p:PublishSingleFile=true",
                "/p:SkipAppPostBuild=true",
                $"/p:CustomAfterMicrosoftCommonTargets={targetsPath}"
            })
            {
                processStartInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start the publish process.");
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
                    await process.WaitForExitAsync();
                }
                catch (Exception killEx)
                {
                    Console.WriteLine($"Error killing the process: {killEx.Message}");
                }

                Console.WriteLine("Publish process was canceled.");
                return false;
            }
            string output = await outputTask;
            string errors = await errorTask;

            // Log errors if any
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine($"Publish Errors: {errors}");
            }
            // Wait for the process to fully exit
            await exitTask;

            // Return true if the build process was successful, false otherwise
            bool success = process.ExitCode == 0;
            if (!success)
            {
                Console.WriteLine($"Publish output: {output}");
            }
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while publishing the project: {ex.Message}");
            return false;
        }
        finally
        {
            if (targetsPath is not null)
            {
                try
                {
                    File.Delete(targetsPath);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Console.WriteLine($"Could not remove temporary publish targets: {ex.Message}");
                }
            }
        }
    }
}

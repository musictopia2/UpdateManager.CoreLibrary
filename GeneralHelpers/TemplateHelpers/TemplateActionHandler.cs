namespace UpdateManager.CoreLibrary.GeneralHelpers.TemplateHelpers;
public static class TemplateActionHandler
{
    public static async Task<bool> RunTemplateActionAsync(string templateDirectory, EnumTemplateAction action, CancellationToken cancellationToken = default)
    {
        // Validate template directory
        if (string.IsNullOrWhiteSpace(templateDirectory) || !Directory.Exists(templateDirectory))
        {
            Console.WriteLine($"Invalid template directory: {templateDirectory}");
            return false;
        }
        try
        {
            string startArgument = action switch
            {
                EnumTemplateAction.Uninstall => "uninstall",
                EnumTemplateAction.Install => "install",
                _ => throw new ArgumentException("Invalid template action", nameof(action))
            };
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"""
                new {startArgument} "{templateDirectory}"
                """,
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
            string output = await process.StandardOutput.ReadToEndAsync();
            string errors = await process.StandardError.ReadToEndAsync();
            //Console.WriteLine(output);
            if (!string.IsNullOrEmpty(errors))
            {
                //Console.WriteLine($"Errors: {errors}");
            }

            // Wait for the process to exit and check the exit code
            await process.WaitForExitAsync(cancellationToken);
            bool rets;
            rets = process.ExitCode == 0;
            if (rets == false)
            {
                Console.WriteLine(output);
            }
            return rets;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while doing template action: {ex.Message}");
            return false;
        }
    }
}
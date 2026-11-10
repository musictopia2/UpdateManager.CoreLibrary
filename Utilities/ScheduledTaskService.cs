using System.Text;
using System.Text.Json;

namespace UpdateManager.CoreLibrary.Utilities;
public class ScheduledTaskService : IScheduledTaskService
{
    public async Task<ScheduledTaskOriginalStateModel> GetTaskAsync(
        string taskPath,
        string taskName)
    {
        string payLoad = await ExecuteAsync(
            taskPath,
            taskName,
            enabled: null);
        ScheduledTaskOriginalStateModel output = await jj2.DeserializeObjectAsync<ScheduledTaskOriginalStateModel>(payLoad);
        return output;
    }

    public async Task SetEnabledAsync(
        string taskPath,
        string taskName,
        bool enabled)
    {
        await ExecuteAsync(taskPath, taskName, enabled);
    }

    private static async Task<string> ExecuteAsync(
        string taskPath,
        string taskName,
        bool? enabled)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "Scheduled task management requires Windows.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(taskPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskName);

        string payload = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new
                {
                    TaskPath = taskPath,
                    TaskName = taskName,
                    Enabled = enabled
                })));

        string script = $$"""
            $ErrorActionPreference = 'Stop'
            $ProgressPreference = 'SilentlyContinue'
            [Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)

            try
            {
                $request = [Text.Encoding]::UTF8.GetString(
                    [Convert]::FromBase64String('{{payload}}')
                ) | ConvertFrom-Json

                $tasks = @(
                    Get-ScheduledTask `
                        -TaskPath $request.TaskPath `
                        -TaskName $request.TaskName `
                        -ErrorAction Stop |
                    Where-Object {
                        $_.TaskPath -eq $request.TaskPath -and
                        $_.TaskName -eq $request.TaskName
                    }
                )

                if ($tasks.Count -ne 1)
                {
                    throw "Expected exactly one matching scheduled task; found $($tasks.Count)."
                }

                $task = $tasks[0]

                if ($null -ne $request.Enabled)
                {
                    if ([bool]$request.Enabled)
                    {
                        $task | Enable-ScheduledTask -ErrorAction Stop |
                            Out-Null
                    }
                    else
                    {
                        $task | Disable-ScheduledTask -ErrorAction Stop |
                            Out-Null
                    }

                    $task = Get-ScheduledTask `
                        -TaskPath $request.TaskPath `
                        -TaskName $request.TaskName `
                        -ErrorAction Stop

                    if ([bool]$task.Settings.Enabled -ne [bool]$request.Enabled)
                    {
                        throw 'The task enabled setting did not match the requested value.'
                    }
                }
                $scheduler = New-Object -ComObject 'Schedule.Service'
                $scheduler.Connect()

                $folder = $scheduler.GetFolder([string]$request.TaskPath)
                $registeredTask = $folder.GetTask([string]$request.TaskName)
                $runningInstances = $registeredTask.GetInstances(0)
                [pscustomobject]@{
                    TaskName = [string]$task.TaskName
                    TaskPath = [string]$task.TaskPath
                    Enabled = [bool]$task.Settings.Enabled
                    State = $task.State.ToString()
                    RunningInstanceCount = [int]$runningInstances.Count
            } | ConvertTo-Json -Compress

                exit 0
            }
            catch
            {
                [Console]::Error.WriteLine($_.Exception.Message)
                exit 1
            }
            """;

        string powershell = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = powershell,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-EncodedCommand");
        startInfo.ArgumentList.Add(
            Convert.ToBase64String(
                Encoding.Unicode.GetBytes(script)));

        using var process = new Process
        {
            StartInfo = startInfo
        };

        if (!process.Start())
        {
            throw new InvalidOperationException(
                "Could not start scheduled task management.");
        }

        Task<string> outputTask =
            process.StandardOutput.ReadToEndAsync();

        Task<string> errorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Scheduled task operation failed for " +
                $"'{taskPath}{taskName}': {error.Trim()}");
        }

        return output.Trim();
    }
}
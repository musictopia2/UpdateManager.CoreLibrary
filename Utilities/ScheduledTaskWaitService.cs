namespace UpdateManager.CoreLibrary.Utilities;
public class ScheduledTaskWaitService(
    IScheduledTaskService taskService)
{
    public async Task<bool> WaitAsync(
        IEnumerable<ScheduledTaskSnapshotModel> tasks,
        TimeSpan? interval = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var snapshot = tasks.ToArray();

        if (snapshot.Length == 0)
        {
            throw new InvalidOperationException(
                "No scheduled tasks were supplied for waiting.");
        }

        return await WaitHelper.WaitUntilAsync(
            async token =>
            {
                bool allFinished = true;

                foreach (var item in snapshot)
                {
                    token.ThrowIfCancellationRequested();

                    var current = await taskService.GetTaskAsync(
                        item.TaskPath,
                        item.TaskName);

                    token.ThrowIfCancellationRequested();

                    if (current.Enabled)
                    {
                        throw new InvalidOperationException(
                            $"Task '{item.TaskName}' must be disabled " +
                            "before waiting for its running jobs.");
                    }

                    if (current.RunningInstanceCount > 0)
                    {
                        Console.WriteLine(
                            $"Waiting for '{item.TaskName}': " +
                            $"{current.RunningInstanceCount} running.");

                        allFinished = false;
                    }
                }

                return allFinished;
            },
            interval,
            timeout,
            cancellationToken);
    }
}
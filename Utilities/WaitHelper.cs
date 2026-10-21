namespace UpdateManager.CoreLibrary.Utilities;
public static class WaitHelper
{
    public static string Status { get; set; } = "pending items";
    public static async Task<bool> WaitUntilAsync(
    Func<CancellationToken, Task<bool>> condition,
    TimeSpan? interval = null,
    TimeSpan? timeout = null,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        TimeSpan actualInterval = interval ?? TimeSpan.FromMinutes(1);
        TimeSpan actualTimeout = timeout ?? TimeSpan.FromMinutes(45);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            actualInterval, TimeSpan.Zero);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            actualTimeout, TimeSpan.Zero);

        using CancellationTokenSource timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeoutSource.CancelAfter(actualTimeout);

        try
        {
            while (true)
            {
                timeoutSource.Token.ThrowIfCancellationRequested();
                Console.WriteLine($"Checking {Status} to see if they are available on {DateTime.Now}");
                
                if (await condition(timeoutSource.Token))
                {
                    return true;
                }
                
                await Task.Delay(actualInterval, timeoutSource.Token);
            }
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested &&
                  timeoutSource.IsCancellationRequested)
        {
            return false;
        }
    }
}
namespace UpdateManager.CoreLibrary.Interfaces;
public interface IScheduledTaskService
{
    Task<ScheduledTaskOriginalStateModel> GetTaskAsync(
        string taskPath,
        string taskName);

    Task SetEnabledAsync(
        string taskPath,
        string taskName,
        bool enabled);
}
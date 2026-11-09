namespace UpdateManager.CoreLibrary.Models;
public class ScheduledTaskSnapshotModel
{
    public string TaskName { get; set; } = "";
    public string TaskPath { get; set; } = "";
    public bool OriginallyEnabled { get; set; }
}
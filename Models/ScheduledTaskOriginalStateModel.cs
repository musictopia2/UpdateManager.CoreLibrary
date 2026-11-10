namespace UpdateManager.CoreLibrary.Models;
public class ScheduledTaskOriginalStateModel
{
    public string TaskName { get; set; } = "";
    public string TaskPath { get; set; } = "";
    public bool Enabled { get; set; }
    public string State { get; set; } = "";
    public int RunningInstanceCount { get; set; }
}
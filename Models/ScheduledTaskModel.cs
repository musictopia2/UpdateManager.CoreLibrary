namespace UpdateManager.CoreLibrary.Models;
public class ScheduledTaskModel
{
    public string TaskName { get; set; } = "";
    public string TaskPath { get; set; } = "";
    public bool Enabled { get; set; }
    public string State { get; set; } = "";
}
namespace UpdateManager.CoreLibrary.Models;
public class ShortcutModel
{
    public string ShortcutName { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public string ProjectName { get; set; } = ""; //so i can match up.
}
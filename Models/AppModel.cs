namespace UpdateManager.CoreLibrary.Models;
public class AppModel
{
    public string ProjectName { get; set; } = "";
    public string ShortcutName { get; set; } = "";
    //this is used if someone on your network uses the app.
    public string NetworkedPath { get; set; } = "";
    public string CsProj { get; set; } = "";
    public bool WindowsWPFBlazor { get; set; }
    public string Category { get; set; } = ""; //this is used so i can specify to update only a category of apps.
    public bool Deleted { get; set; } //if i mark as deleted, then won't update.  however, would still show up so when discovering apps, won't add again.
}
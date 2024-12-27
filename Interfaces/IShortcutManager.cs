namespace UpdateManager.CoreLibrary.Interfaces;
public interface IShortcutManager
{
    BasicList<ShortcutModel> ListShortcuts();
    void UpdateShortcut(string name, string newTargetPath);
}
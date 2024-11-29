namespace UpdateManager.CoreLibrary.YearlyNetUpgradeHelpers.TestHelpers;
public class TestFileManager : ITestFileManager
{
    public bool FileExists(string filePath) => File.Exists(filePath);
    public void CopyFile(string sourcePath, string destinationPath)
    {
        File.Copy(sourcePath, destinationPath, true);
    }
    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
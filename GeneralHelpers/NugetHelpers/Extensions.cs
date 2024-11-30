namespace UpdateManager.CoreLibrary.GeneralHelpers.NugetHelpers;
public static class Extensions
{
    public static async Task<string> GetNugetFileAsync(this string nugetPath)
    {
        string filePath = await ff1.GetSpecificFileAsync(nugetPath, ".nupkg");
        return filePath;
    }
}
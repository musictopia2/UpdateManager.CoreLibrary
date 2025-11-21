namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class Extensions
{
    extension(string nugetPath)
    {
        public async Task<string> GetNugetFileAsync()
        {
            string filePath = await ff1.GetSpecificFileAsync(nugetPath, ".nupkg");
            return filePath;
        }
    }  
}
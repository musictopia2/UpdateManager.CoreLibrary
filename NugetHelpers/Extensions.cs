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
    extension(string value)
    {
        public bool IsLocalFeed
        {
            get
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new CustomBasicException("Must have a value");
                }

                // UNC path: \\server\share\...
                if (value.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Drive path: C:\... or C:/...
                if (value.Length >= 3 &&
                    char.IsLetter(value[0]) &&
                    value[1] == ':' &&
                    (value[2] == '\\' || value[2] == '/'))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
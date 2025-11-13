using System.IO.Compression; //not common enough
using System.Text.Json;
namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetPackageChecker
{
    private static readonly HttpClient _client = new();
    public static async Task<string> GetLatestPublicVersionAsync(string packageName)
    {
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";

        try
        {
            HttpResponseMessage response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: Failed to query NuGet. Status Code: {response.StatusCode}");
                return null!;
            }

            bool isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");
            string responseContent = await ReadResponseContentAsync(response, isGzip);

            using JsonDocument doc = JsonDocument.Parse(responseContent);

            if (!doc.RootElement.TryGetProperty("items", out JsonElement outerItems))
            {
                Console.WriteLine("Error: 'items' not found in index.");
                return null!;
            }

            List<string> allStableVersions = [];

            // Loop through pages (outer items)
            foreach (var outerItem in outerItems.EnumerateArray())
            {
                // Case 1: inline inner items (old-style)
                if (outerItem.TryGetProperty("items", out JsonElement innerItems))
                {
                    foreach (var packageEntry in innerItems.EnumerateArray())
                    {
                        if (TryExtractStableVersion(packageEntry, out string? version))
                        {
                            allStableVersions.Add(version!);
                        }
                    }
                }
                // Case 2: paged sub-documents (new-style)
                else if (outerItem.TryGetProperty("@id", out JsonElement pageUrlElement))
                {
                    string pageUrl = pageUrlElement.GetString()!;
                    HttpResponseMessage subPageResponse = await _client.GetAsync(pageUrl);
                    if (!subPageResponse.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    string subPageContent = await ReadResponseContentAsync(subPageResponse,
                        subPageResponse.Content.Headers.ContentEncoding.Contains("gzip"));

                    using JsonDocument subDoc = JsonDocument.Parse(subPageContent);
                    if (subDoc.RootElement.TryGetProperty("items", out JsonElement subItems))
                    {
                        foreach (var packageEntry in subItems.EnumerateArray())
                        {
                            if (TryExtractStableVersion(packageEntry, out string? version))
                            {
                                allStableVersions.Add(version!);
                            }
                        }
                    }
                }
            }

            if (allStableVersions.Count == 0)
            {
                Console.WriteLine("No stable versions found.");
                return null!;
            }

            // Determine the numerically highest version manually
            string latest = allStableVersions
                .Select(ParseVersion)
                .OrderByDescending(v => v, new VersionComparer())
                .First().Original;

            return latest;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking latest version: {ex.Message}");
            return null!;
        }
    }

    private static bool TryExtractStableVersion(JsonElement packageEntry, out string? version)
    {
        version = null;

        if (packageEntry.TryGetProperty("catalogEntry", out JsonElement catalogEntry) &&
            catalogEntry.TryGetProperty("version", out JsonElement versionElement))
        {
            string v = versionElement.GetString()!;
            if (!IsPreReleaseVersion(v))
            {
                version = v;
                return true;
            }
        }

        return false;
    }

    private record ParsedVersion(int[] Parts, string Original);

    private static ParsedVersion ParseVersion(string version)
    {
        string cleaned = version.Split('-')[0]; // Remove suffix
        string[] parts = cleaned.Split('.', StringSplitOptions.RemoveEmptyEntries);
        int[] nums = [.. parts.Select(p => int.TryParse(p, out int n) ? n : 0)];
        return new ParsedVersion(nums, version);
    }

    private class VersionComparer : IComparer<ParsedVersion>
    {
        public int Compare(ParsedVersion? x, ParsedVersion? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            int len = Math.Max(x.Parts.Length, y.Parts.Length);
            for (int i = 0; i < len; i++)
            {
                int a = i < x.Parts.Length ? x.Parts[i] : 0;
                int b = i < y.Parts.Length ? y.Parts[i] : 0;
                if (a != b)
                {
                    return a.CompareTo(b);
                }
            }
            return 0;
        }
    }


    // This method handles the old format, which looks for the 'upper' property in the 'items' array
    private static async Task<string> GetLatestPublicVersionFromOldFormat(string packageName)
    {
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";
        try
        {
            string latestVersion = "";
            HttpResponseMessage response = await _client.GetAsync(url);

            // Check if the response is successful
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: Failed to query NuGet. Status Code: {response.StatusCode}");
                return null!;
            }

            // Check if the response is GZipped (compressed)
            bool isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");

            // Read and decompress the response content if necessary
            string responseContent = await ReadResponseContentAsync(response, isGzip);

            using JsonDocument doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("items", out JsonElement items))
            {
                // Get the last item from the "items" array manually (since LastOrDefault() doesn't exist for JsonElement)
                JsonElement latestItem = default;
                foreach (var item in items.EnumerateArray())
                {
                    latestItem = item;


                    if (latestItem.ValueKind == JsonValueKind.Object)
                    {
                        if (latestItem.TryGetProperty("upper", out JsonElement upperVersion))
                        {
                            string possibleVersion = upperVersion.GetString()!;
                            if (IsPreReleaseVersion(possibleVersion) == false)
                            {

                                latestVersion = possibleVersion;

                            }
                        }
                        //else
                        //{
                        //    Console.WriteLine("Error: 'upper' version not found in the response (old format).");
                        //    return null!;
                        //}
                    }
                    //return null;

                }
                return latestVersion;


            }
            else
            {
                Console.WriteLine("Error: 'items' property not found in the response (old format).");
                return null!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking latest version in old format: {ex.Message}");
            return null!;
        }
    }



    // Check if the version contains a pre-release identifier (e.g., "-beta", "-alpha", "-preview")
    private static bool IsPreReleaseVersion(string version)
    {
        return version.Contains('-') || version.Contains("preview", StringComparison.OrdinalIgnoreCase);
    }

    // Compare version strings using the Version class to compare numerically
    private static int CompareVersions(string version1, string version2)
    {
        var v1 = new Version(version1);
        var v2 = new Version(version2);
        return v1.CompareTo(v2);
    }

    public static async Task<bool> IsPublicPackageAvailableAsync(string packageName, string version)
    {
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";

        try
        {
            HttpResponseMessage response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: Failed to query NuGet. Status Code: {response.StatusCode}");
                return false;
            }

            bool isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");
            string responseContent = await ReadResponseContentAsync(response, isGzip);

            using JsonDocument doc = JsonDocument.Parse(responseContent);

            if (!doc.RootElement.TryGetProperty("items", out JsonElement outerItems))
            {
                return false;
            }

            foreach (var outerItem in outerItems.EnumerateArray().Reverse())
            {
                if (outerItem.TryGetProperty("items", out JsonElement innerItems))
                {
                    // Old-style: inner items are inlined
                    foreach (var packageEntry in innerItems.EnumerateArray().Reverse())
                    {
                        if (CheckPackageEntry(packageEntry, version))
                        {
                            return true;
                        }
                    }
                }
                else if (outerItem.TryGetProperty("@id", out JsonElement pageUrlElement))
                {
                    // New-style: sub-page must be fetched
                    string pageUrl = pageUrlElement.GetString()!;
                    HttpResponseMessage subPageResponse = await _client.GetAsync(pageUrl);

                    if (!subPageResponse.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    string subPageContent = await ReadResponseContentAsync(subPageResponse, subPageResponse.Content.Headers.ContentEncoding.Contains("gzip"));
                    using JsonDocument subDoc = JsonDocument.Parse(subPageContent);

                    if (subDoc.RootElement.TryGetProperty("items", out JsonElement subItems))
                    {
                        foreach (var packageEntry in subItems.EnumerateArray().Reverse())
                        {
                            if (CheckPackageEntry(packageEntry, version))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking package version availability: {ex.Message}");
            return false;
        }
    }

    private static bool CheckPackageEntry(JsonElement packageEntry, string version)
    {
        if (packageEntry.TryGetProperty("catalogEntry", out JsonElement catalogEntry))
        {
            if (catalogEntry.TryGetProperty("version", out JsonElement entryVersion))
            {
                string versionStr = entryVersion.GetString()!;
                if (string.Equals(versionStr, version, StringComparison.OrdinalIgnoreCase))
                {
                    bool isListed = catalogEntry.TryGetProperty("listed", out JsonElement listedElement) && listedElement.GetBoolean();
                    if (!isListed)
                    {
                        return false;
                    }

                    if (catalogEntry.TryGetProperty("published", out JsonElement publishedElement) &&
                        DateTime.TryParse(publishedElement.GetString(), out DateTime publishedDate) &&
                        publishedDate.Year < 2000)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        return false;
    }
    private static async Task<string> ReadResponseContentAsync(HttpResponseMessage response, bool isGzip)
    {
        // If the response is gzipped, we need to decompress it
        if (isGzip)
        {
            using var gzipStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            return await reader.ReadToEndAsync();
        }
        else
        {
            // If not gzipped, read it directly
            return await response.Content.ReadAsStringAsync();
        }
    }
    public static async Task WaitForPublicPackageToBeAvailable(string packageName, string version, int maxRetries = 10, int delayInSeconds = 60)
    {
        bool packageAvailable = false;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            packageAvailable = await IsPublicPackageAvailableAsync(packageName, version);

            if (packageAvailable)
            {
                Console.WriteLine($"Package {packageName} version {version} is now available on NuGet.");
                break;
            }

            Console.WriteLine($"Attempt {attempt} failed. Retrying in {delayInSeconds} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
        }

        if (!packageAvailable)
        {
            Console.WriteLine($"Failed to find package {packageName} version {version} after {maxRetries} attempts.");
            // Handle failure logic here (e.g., alert, retry, etc.)
        }
    }

}
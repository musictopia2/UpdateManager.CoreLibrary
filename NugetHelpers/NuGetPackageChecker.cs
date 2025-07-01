using System.IO.Compression; //not common enough
using System.Text.Json;
namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetPackageChecker
{
    private static readonly HttpClient client = new();
    public static async Task<string> GetLatestPublicVersionAsync(string packageName)
    {
        string latestVersion = "";
        // Construct the URL to the NuGet Registration API (for SemVer 2 packages)
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";

        try
        {
            // Send an HTTP GET request to the NuGet Registration API
            HttpResponseMessage response = await client.GetAsync(url);

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

            // Parse the JSON response
            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseContent);

                // Try the newer format first
                if (doc.RootElement.TryGetProperty("items", out JsonElement items) && items.GetArrayLength() > 0)
                {
                    // Iterate through each item and collect all versions
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("items", out JsonElement versions) && versions.GetArrayLength() > 0)
                        {
                            // Iterate through the versions in this "items" array
                            foreach (var versionItem in versions.EnumerateArray())
                            {
                                // Extract the "version" directly from catalogEntry
                                if (versionItem.TryGetProperty("catalogEntry", out JsonElement catalogEntry))
                                {
                                    if (catalogEntry.TryGetProperty("version", out JsonElement version))
                                    {
                                        string versionValue = version.GetString()!;
                                        if (!IsPreReleaseVersion(versionValue)) // Only add stable versions
                                        {
                                            latestVersion = versionValue;
                                            //break; // Since the versions are sorted, the first one is the latest
                                        }
                                    }
                                }
                            }
                        }

                        // If we have found a valid latest version, no need to continue checking
                        //if (!string.IsNullOrEmpty(latestVersion))
                        //{
                        //    break;
                        //}
                    }
                }
                // If no version is found using the new format, try the old format
                if (string.IsNullOrEmpty(latestVersion))
                {
                    latestVersion = await GetLatestPublicVersionFromOldFormat(packageName);
                }
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"Error parsing JSON response: {parseEx.Message}");
                return null!;
            }

            return latestVersion;
        }
        catch (Exception ex)
        {
            // Handle any exceptions (e.g., network issues, JSON parsing errors)
            Console.WriteLine($"Error checking latest version: {ex.Message}");
            return null!;
        }
    }

    // This method handles the old format, which looks for the 'upper' property in the 'items' array
    private static async Task<string> GetLatestPublicVersionFromOldFormat(string packageName)
    {
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";
        try
        {
            string latestVersion = "";
            HttpResponseMessage response = await client.GetAsync(url);

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
        return version.Contains("-") || version.Contains("preview", StringComparison.OrdinalIgnoreCase);
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
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: Failed to query NuGet. Status Code: {response.StatusCode}");
                return false;
            }

            bool isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");
            string responseContent = await ReadResponseContentAsync(response, isGzip);

            using JsonDocument doc = JsonDocument.Parse(responseContent);

            if (doc.RootElement.TryGetProperty("items", out JsonElement outerItems))
            {
                // Materialize outer items and reverse
                var outerItemList = outerItems.EnumerateArray().Reverse().ToList();

                foreach (var outerItem in outerItemList)
                {
                    if (outerItem.TryGetProperty("items", out JsonElement innerItems))
                    {
                        // Materialize inner items and reverse
                        var innerItemList = innerItems.EnumerateArray().Reverse().ToList();

                        foreach (var packageEntry in innerItemList)
                        {
                            if (packageEntry.TryGetProperty("catalogEntry", out JsonElement catalogEntry))
                            {
                                if (catalogEntry.TryGetProperty("version", out JsonElement entryVersion))
                                {
                                    string versionStr = entryVersion.GetString()!;
                                    if (string.Equals(versionStr, version, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Optional: Skip unlisted or invalid entries
                                        bool isListed = catalogEntry.TryGetProperty("listed", out JsonElement listedElement) && listedElement.GetBoolean();
                                        if (!isListed)
                                        {
                                            //Console.WriteLine($"Found version {version}, but it is unlisted.");
                                            return false;
                                        }

                                        if (catalogEntry.TryGetProperty("published", out JsonElement publishedElement) &&
                                            DateTime.TryParse(publishedElement.GetString(), out DateTime publishedDate) &&
                                            publishedDate.Year < 2000)
                                        {
                                            //Console.WriteLine($"Found version {version}, but it has a suspicious publish date: {publishedDate}");
                                            return false;
                                        }

                                        return true;
                                    }
                                }
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
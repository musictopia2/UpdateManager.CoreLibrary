﻿using System.IO.Compression; //not common enough
using System.Text.Json;
namespace UpdateManager.CoreLibrary.NugetHelpers;
public static class NuGetPackageChecker
{
    private static readonly HttpClient client = new();
    public static async Task<string> GetLatestPublicVersionAsync(string packageName)
    {
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

            // Log the full response to debug what's happening
            //Console.WriteLine("Response Content (raw): ");
            //Console.WriteLine(responseContent);  // Log the raw response content

            // Parse the JSON response
            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                // Log the entire structure to inspect the JSON
                //Console.WriteLine("Parsed JSON Structure:");
                //Console.WriteLine(doc.RootElement.ToString());

                // Navigate to the "items" array in the JSON response
                if (doc.RootElement.TryGetProperty("items", out JsonElement items))
                {
                    // Get the last item from the "items" array manually (since LastOrDefault() doesn't exist for JsonElement)
                    JsonElement latestItem = default;
                    foreach (var item in items.EnumerateArray())
                    {
                        latestItem = item;
                    }

                    // If we have a valid latest item, extract the "upper" version
                    if (latestItem.ValueKind == JsonValueKind.Object)
                    {
                        if (latestItem.TryGetProperty("upper", out JsonElement upperVersion))
                        {
                            string latestVersion = upperVersion.GetString()!;
                            //Console.WriteLine($"The latest version of {packageName} is {latestVersion}");
                            return latestVersion;
                        }
                        else
                        {
                            Console.WriteLine("Error: 'upper' version not found in the response.");
                            return null!;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: 'items' property not found in the response.");
                    return null!;
                }
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"Error parsing JSON response: {parseEx.Message}");
                return null!;
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions (e.g., network issues, JSON parsing errors)
            Console.WriteLine($"Error checking latest version: {ex.Message}");
            return null!;
        }
        return null!;
    }
    public static async Task<bool> IsPublicPackageAvailableAsync(string packageName, string version)
    {
        // Construct the URL dynamically using the package name
        string url = $"https://api.nuget.org/v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";

        try
        {
            // Send an HTTP GET request to the NuGet Registration API
            HttpResponseMessage response = await client.GetAsync(url);
            // Check if the response is successful
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: Failed to query NuGet. Status Code: {response.StatusCode}");
                return false;
            }

            // Check if the response is GZipped (compressed)
            bool isGzip = response.Content.Headers.ContentEncoding.Contains("gzip");

            // Read and decompress the response content if necessary
            string responseContent = await ReadResponseContentAsync(response, isGzip);
            // Parse the JSON response using System.Text.Json
            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseContent);

                // Navigate to the "items" array (same parsing as in GetLatestPublicVersionAsync)
                if (doc.RootElement.TryGetProperty("items", out JsonElement items))
                {
                    JsonElement latestItem = default;
                    // Get the last item from the "items" array manually (same as GetLatestPublicVersionAsync)
                    foreach (var item in items.EnumerateArray())
                    {
                        latestItem = item;
                    }

                    // If we have a valid latest item, extract the "upper" version
                    if (latestItem.ValueKind == JsonValueKind.Object)
                    {
                        if (latestItem.TryGetProperty("upper", out JsonElement upperVersion))
                        {
                            string latestVersion = upperVersion.GetString()!;
                            // Log the found version to debug

                            // Now compare the latest version with the requested version
                            if (string.Equals(latestVersion.Trim(), version.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"Error parsing JSON response: {parseEx.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking package version availability: {ex.Message}");
            return false;
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
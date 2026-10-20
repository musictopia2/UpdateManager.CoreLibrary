namespace UpdateManager.CoreLibrary.NugetHelpers;

public interface IGitHubNugetConfiguration
{
    string Repository { get; }
    string Workflow { get; }
    TimeSpan Timeout { get; }
}
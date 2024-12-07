namespace UpdateManager.CoreLibrary.PostBuildHelpers;
public record struct PostBuildArguments(string ProjectName, string ProjectDirectory, string ProjectFile, string OutputDirectory);
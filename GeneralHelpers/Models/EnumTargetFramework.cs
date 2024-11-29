namespace UpdateManager.CoreLibrary.GeneralHelpers.Models;
public enum EnumTargetFramework
{
    NetStandard, //For libraries targeting .NET Standard (no need for upgrades).
    NetRuntime //For libraries targeting .NET 5/6/7/8+ (can be upgraded).
}
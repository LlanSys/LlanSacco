namespace LS.UI.Blazor.Configuration;

internal sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    public string ApplicationName { get; init; } = "LlanSacco";
    public string? KeysPath { get; init; }
}

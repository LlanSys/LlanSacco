namespace LS.Tests.Architecture.Guardrails;

internal sealed record ArchitectureFinding(string Rule, string Path, string Symbol, int Line, string Detail, int Phase)
{
    internal string Identity => $"{Rule}|{Path}|{Symbol}|{Detail}";
}

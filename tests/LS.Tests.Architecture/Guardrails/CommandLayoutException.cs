namespace LS.Tests.Architecture.Guardrails;

internal sealed record CommandLayoutException(
    string Symbol,
    string RequestPath,
    string Handler,
    string HandlerPath,
    string Reason);

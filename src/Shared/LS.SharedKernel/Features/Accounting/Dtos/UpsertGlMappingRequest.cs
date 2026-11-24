namespace LS.SharedKernel.Features.Accounting.Dtos;

public record UpsertGlMappingRequest
{
    public string DebitSideSource { get; init; } = string.Empty;
    public Guid? DebitExplicitGlAccountId { get; init; }
    public string CreditSideSource { get; init; } = string.Empty;
    public Guid? CreditExplicitGlAccountId { get; init; }
    public string FeeSideSource { get; init; } = string.Empty;
    public Guid? FeeExplicitGlAccountId { get; init; }
    public string? Description { get; init; }
}

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record GlMappingResponse
{
    public Guid Id { get; init; }
    public required string TransactionTypeCode { get; init; }
    public required string DebitSideSource { get; init; }
    public Guid? DebitExplicitGlAccountId { get; init; }
    public string? DebitAccountCode { get; init; }
    public string? DebitAccountName { get; init; }
    public required string CreditSideSource { get; init; }
    public Guid? CreditExplicitGlAccountId { get; init; }
    public string? CreditAccountCode { get; init; }
    public string? CreditAccountName { get; init; }
    public required string FeeSideSource { get; init; }
    public Guid? FeeExplicitGlAccountId { get; init; }
    public string? FeeAccountCode { get; init; }
    public string? FeeAccountName { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

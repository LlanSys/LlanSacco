namespace LS.Domain.Features.Accounting.Entities;

/// <summary>
/// Ledger account sources a <see cref="TransactionTypeGlMapping"/> side may route to.
/// Posting resolves them in AccountingEventHandlerBase.ResolveAccountId.
/// </summary>
public static class GlMappingSources
{
    public const string ChannelAccount = "CHANNEL_ACCOUNT";
    public const string ProductAccount = "PRODUCT_ACCOUNT";
    public const string ExplicitGl = "EXPLICIT_GL";

    public const string GuarantorRecoveryTransactionType = "GUARANTOR_RECOVERY";

    public static IReadOnlyList<string> All { get; } = [ChannelAccount, ProductAccount, ExplicitGl];

    public static bool IsSupported(string? source) =>
        source is not null && All.Contains(source, StringComparer.Ordinal);
}

namespace LS.Infrastructure.Contracts.Implementations.Common;

public sealed class BackgroundExecutionContext
{
    public Guid? TenantId { get; private set; }
    public string? ActorId { get; private set; }

    public void Initialize(Guid tenantId, string actorId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("A background tenant is required.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        if (TenantId.HasValue) throw new InvalidOperationException("A background execution scope cannot be reassigned.");
        TenantId = tenantId;
        ActorId = actorId;
    }
}

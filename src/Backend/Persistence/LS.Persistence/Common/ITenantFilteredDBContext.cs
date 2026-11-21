namespace LS.Persistence.Common;

internal interface ITenantFilteredDBContext
{
    Guid CurrentTenantId { get; }
}

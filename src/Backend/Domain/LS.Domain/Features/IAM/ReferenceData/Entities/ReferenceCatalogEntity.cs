using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.IAM.ReferenceData.Entities;

public abstract class ReferenceCatalogEntity : BaseEntity, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy);

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy.Trim();
    }
}

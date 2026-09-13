using System;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Collections.Enums;

namespace LS.Domain.Features.Loans.Collections.Entities;

public class CollectionAction : BaseEntity
{
    public Guid CollectionCaseId { get; set; }
    public CollectionActionType ActionType { get; set; }
    public DateTimeOffset ActionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    
    public CollectionCase CollectionCase { get; set; } = null!;
}

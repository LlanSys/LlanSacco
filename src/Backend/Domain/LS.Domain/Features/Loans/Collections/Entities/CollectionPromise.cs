using System;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Collections.Enums;

namespace LS.Domain.Features.Loans.Collections.Entities;

public class CollectionPromise : BaseEntity
{
    public Guid CollectionCaseId { get; set; }
    public DateTimeOffset PromiseDate { get; set; }
    public decimal PromiseAmount { get; set; }
    public PromiseStatus Status { get; set; }
    
    public CollectionCase CollectionCase { get; set; } = null!;
}

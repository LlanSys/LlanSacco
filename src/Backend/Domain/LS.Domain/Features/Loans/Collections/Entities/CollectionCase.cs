using System;
using System.Collections.Generic;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Collections.Enums;
using LS.Domain.Features.Loans.Entities;

namespace LS.Domain.Features.Loans.Collections.Entities;

public class CollectionCase : BaseEntity
{
    public Guid LoanApplicationId { get; set; }
    public Guid? AssignedOfficerUserId { get; set; }
    
    public decimal TotalArrearsAmount { get; set; }
    public int DaysPastDue { get; set; }
    public CollectionCaseStatus Status { get; set; }
    
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionReason { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;
    
    public ICollection<CollectionAction> Actions { get; set; } = new List<CollectionAction>();
    public ICollection<CollectionPromise> Promises { get; set; } = new List<CollectionPromise>();

    public static CollectionCase Create(Guid tenantId, Guid loanApplicationId, decimal arrearsAmount, int daysPastDue, string triggeredBy)
    {
        return new CollectionCase
        {
            TenantId = tenantId,
            LoanApplicationId = loanApplicationId,
            TotalArrearsAmount = arrearsAmount,
            DaysPastDue = daysPastDue,
            Status = CollectionCaseStatus.Open,
            OpenedAt = DateTimeOffset.UtcNow,
            CreatedBy = triggeredBy
        };
    }
}

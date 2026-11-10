using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.CheckOff.Entities;

public class CheckoffStagingLoanAllocation : BaseEntity
{
    public Guid CheckoffStagingRowId { get; set; }
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    
    // Navigation
    public CheckoffStagingRow StagingRow { get; set; } = null!;
    
    public static CheckoffStagingLoanAllocation Create(
        Guid tenantId,
        Guid checkoffStagingRowId,
        Guid loanId,
        decimal amount,
        string createdBy)
    {
        return new CheckoffStagingLoanAllocation
        {
            TenantId = tenantId,
            CheckoffStagingRowId = checkoffStagingRowId,
            LoanId = loanId,
            Amount = amount,
            CreatedBy = createdBy
        };
    }
}

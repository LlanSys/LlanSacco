using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Accounting.Entities;

public class JournalLine : BaseEntity
{
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }

    public Journal Journal { get; set; } = null!;
    public Account Account { get; set; } = null!;

    public static JournalLine Create(
        Guid tenantId,
        Guid journalId,
        Guid accountId,
        string? description,
        decimal debit,
        decimal credit,
        string createdBy,
        Guid? branchId = null,
        Guid? costCenterId = null)
    {
        return new JournalLine
        {
            TenantId = tenantId,
            JournalId = journalId,
            AccountId = accountId,
            Description = description,
            Debit = debit,
            Credit = credit,
            BranchId = branchId,
            CostCenterId = costCenterId,
            CreatedBy = createdBy
        };
    }
}

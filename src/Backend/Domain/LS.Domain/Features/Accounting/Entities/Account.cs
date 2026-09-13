using LS.Domain.Shared.Entities;
using LS.Domain.Features.Accounting.Enums;
using System;
using System.Collections.Generic;

namespace LS.Domain.Features.Accounting.Entities;

public class Account : BaseEntity
{
    public required string AccountCode { get; set; }
    public required string AccountName { get; set; }
    public AccountType AccountType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();

    public static Account Create(
        Guid tenantId,
        string accountCode,
        string accountName,
        AccountType accountType,
        string? description,
        string createdBy)
    {
        return new Account
        {
            TenantId = tenantId,
            AccountCode = accountCode,
            AccountName = accountName,
            AccountType = accountType,
            Description = description,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}

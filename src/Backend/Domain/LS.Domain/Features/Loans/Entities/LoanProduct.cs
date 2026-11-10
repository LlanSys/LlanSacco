using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Enums;
using System;

namespace LS.Domain.Features.Loans.Entities;

public class LoanProduct : BaseEntity
{
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public InterestMethod InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }

    public static LoanProduct Create(
        Guid tenantId,
        string productCode,
        string productName,
        string? description,
        decimal interestRate,
        InterestMethod interestMethod,
        int maxTermInMonths,
        decimal maxAmount,
        string createdBy)
    {
        return new LoanProduct
        {
            TenantId = tenantId,
            ProductCode = productCode,
            ProductName = productName,
            Description = description,
            InterestRate = interestRate,
            InterestMethod = interestMethod,
            MaxTermInMonths = maxTermInMonths,
            MaxAmount = maxAmount,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}

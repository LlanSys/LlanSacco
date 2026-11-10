using System;
using LS.Domain.Features.HR.Payroll.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class PayslipDetail : BaseEntity
{
    public Guid PayslipId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public PayslipDetailType Type { get; private set; }

    private PayslipDetail() { }

    internal static PayslipDetail Create(Guid payslipId, string description, decimal amount, PayslipDetailType type, string createdBy)
    {
        return new PayslipDetail
        {
            Id = Guid.CreateVersion7(),
            PayslipId = payslipId,
            Description = description,
            Amount = amount,
            Type = type,
            CreatedBy = createdBy
        };
    }
}

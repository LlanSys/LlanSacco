using System;
using LS.Domain.Features.HR.Payroll.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class PayrollPeriod : BaseEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public PayrollPeriodStatus Status { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? ProcessedBy { get; private set; }

    private PayrollPeriod() { }

    public static PayrollPeriod Create(int year, int month, string createdBy)
    {
        return new PayrollPeriod
        {
            Id = Guid.CreateVersion7(),
            Year = year,
            Month = month,
            Status = PayrollPeriodStatus.Open,
            CreatedBy = createdBy
        };
    }

    public void RecordPayrollRun(string actorId)
    {
        if (Status != PayrollPeriodStatus.Open || ProcessedAt is not null)
            throw new InvalidOperationException("Payroll cannot be generated again for this period.");
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessedBy = actorId;
        SetUpdatedInfo(actorId);
    }

    public void Close(string processedBy)
    {
        if (Status == PayrollPeriodStatus.Closed)
        {
            throw new InvalidOperationException("Payroll period is already closed.");
        }

        Status = PayrollPeriodStatus.Closed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessedBy = processedBy;
        SetUpdatedInfo(processedBy);
    }
}

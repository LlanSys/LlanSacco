using LS.Domain.Features.HR.Payroll.Entities;
using LS.Domain.Features.HR.Payroll.Enums;

namespace LS.Tests.Unit.Domain.Features.HR.Payroll.Entities;

public sealed class PayrollPeriodTests
{
    [Fact]
    public void A_recorded_run_preserves_its_actor_and_cannot_be_overwritten()
    {
        var actorId = Guid.CreateVersion7().ToString();
        var period = PayrollPeriod.Create(2025, 1, actorId);
        period.RecordPayrollRun(actorId);
        var processedAt = period.ProcessedAt;
        Assert.NotNull(processedAt);
        Assert.Equal(PayrollPeriodStatus.Open, period.Status);
        Assert.Throws<InvalidOperationException>(() => period.RecordPayrollRun(Guid.CreateVersion7().ToString()));
        Assert.Equal(actorId, period.ProcessedBy);
        Assert.Equal(processedAt, period.ProcessedAt);
    }

    [Fact]
    public void A_closed_period_cannot_record_a_new_payroll_run()
    {
        var actorId = Guid.CreateVersion7().ToString();
        var period = PayrollPeriod.Create(2025, 1, actorId);
        period.Close(actorId);
        Assert.Throws<InvalidOperationException>(() => period.RecordPayrollRun(actorId));
    }
}

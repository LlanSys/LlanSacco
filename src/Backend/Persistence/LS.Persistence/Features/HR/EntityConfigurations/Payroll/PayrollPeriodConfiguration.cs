using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("Hr_PayrollPeriods");
        builder.Property(x => x.ProcessedAt).IsConcurrencyToken();
        builder.HasKey(x => x.Id);
    }
}

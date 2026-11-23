using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayrollComponentConfiguration : IEntityTypeConfiguration<PayrollComponent>
{
    public void Configure(EntityTypeBuilder<PayrollComponent> builder)
    {
        builder.ToTable("Hr_PayrollComponents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

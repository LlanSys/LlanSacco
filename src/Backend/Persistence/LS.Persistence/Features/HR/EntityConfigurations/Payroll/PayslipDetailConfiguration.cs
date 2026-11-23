using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayslipDetailConfiguration : IEntityTypeConfiguration<PayslipDetail>
{
    public void Configure(EntityTypeBuilder<PayslipDetail> builder)
    {
        builder.ToTable("Hr_PayslipDetails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(150);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}

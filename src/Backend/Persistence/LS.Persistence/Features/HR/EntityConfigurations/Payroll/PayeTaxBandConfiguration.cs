using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayeTaxBandConfiguration : IEntityTypeConfiguration<PayeTaxBand>
{
    public void Configure(EntityTypeBuilder<PayeTaxBand> builder)
    {
        builder.ToTable("Hr_PayeTaxBands");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.LowerLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.UpperLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Rate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.DeletedBy).HasMaxLength(256);
    }
}

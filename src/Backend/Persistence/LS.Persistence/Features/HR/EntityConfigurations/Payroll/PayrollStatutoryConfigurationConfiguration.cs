using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayrollStatutoryConfigurationConfiguration : IEntityTypeConfiguration<PayrollStatutoryConfiguration>
{
    public void Configure(EntityTypeBuilder<PayrollStatutoryConfiguration> builder)
    {
        builder.ToTable("Hr_PayrollStatutoryConfigurations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NssfTier1Limit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NssfTier2Limit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NssfRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.ShifRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.HousingLevyRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.PersonalReliefAmount).HasColumnType("decimal(18,2)");

        builder.Navigation(x => x.PayeTaxBands).AutoInclude();
        builder.HasMany(x => x.PayeTaxBands)
               .WithOne()
               .HasForeignKey(x => x.ConfigurationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Hr_Payslips");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAllowances).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrossPay).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PayeAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NssfAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShifAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.HousingLevyAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NetPay).HasColumnType("decimal(18,2)");

        builder.HasMany(x => x.Details)
               .WithOne()
               .HasForeignKey(x => x.PayslipId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

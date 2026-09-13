using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal class PayrollStatutoryConfigurationTypeConfig : IEntityTypeConfiguration<PayrollStatutoryConfiguration>
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

internal class PayeTaxBandTypeConfig : IEntityTypeConfiguration<PayeTaxBand>
{
    public void Configure(EntityTypeBuilder<PayeTaxBand> builder)
    {
        builder.ToTable("Hr_PayeTaxBands");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LowerLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.UpperLimit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Rate).HasColumnType("decimal(5,2)");
    }
}

internal class PayrollPeriodTypeConfig : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("Hr_PayrollPeriods");
        builder.Property(x => x.ProcessedAt).IsConcurrencyToken();
        builder.HasKey(x => x.Id);
    }
}

internal class EmployeeSalaryTypeConfig : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("Hr_EmployeeSalaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
    }
}

internal class PayrollComponentTypeConfig : IEntityTypeConfiguration<PayrollComponent>
{
    public void Configure(EntityTypeBuilder<PayrollComponent> builder)
    {
        builder.ToTable("Hr_PayrollComponents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

internal class EmployeePayrollComponentTypeConfig : IEntityTypeConfiguration<EmployeePayrollComponent>
{
    public void Configure(EntityTypeBuilder<EmployeePayrollComponent> builder)
    {
        builder.ToTable("Hr_EmployeePayrollComponents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}

internal class PayslipTypeConfig : IEntityTypeConfiguration<Payslip>
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

internal class PayslipDetailTypeConfig : IEntityTypeConfiguration<PayslipDetail>
{
    public void Configure(EntityTypeBuilder<PayslipDetail> builder)
    {
        builder.ToTable("Hr_PayslipDetails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(150);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}

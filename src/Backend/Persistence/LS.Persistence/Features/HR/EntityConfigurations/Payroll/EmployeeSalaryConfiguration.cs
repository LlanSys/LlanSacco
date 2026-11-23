using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class EmployeeSalaryConfiguration : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("Hr_EmployeeSalaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
    }
}

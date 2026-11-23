using LS.Domain.Features.HR.Payroll.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.HR.EntityConfigurations.Payroll;

internal sealed class EmployeePayrollComponentConfiguration : IEntityTypeConfiguration<EmployeePayrollComponent>
{
    public void Configure(EntityTypeBuilder<EmployeePayrollComponent> builder)
    {
        builder.ToTable("Hr_EmployeePayrollComponents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}

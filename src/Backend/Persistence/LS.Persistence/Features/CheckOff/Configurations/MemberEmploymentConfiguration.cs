using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class MemberEmploymentConfiguration : IEntityTypeConfiguration<MemberEmployment>
{
    public void Configure(EntityTypeBuilder<MemberEmployment> builder)
    {
        

        builder.ToTable("MemberEmployments", "checkoff");

        builder.Property(e => e.EmployeePayrollNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => new { e.TenantId, e.EmployerId, e.EmployeePayrollNumber }).IsUnique();

        // Foreign keys setup requires cross-context knowledge. 
        // We will assume Member table is accessed via its DBContext, but we can't easily configure an FK to a table in another schema/context natively if it's strictly bounded. 
        // We configure it carefully. Usually EF Core works fine if we just specify the HasOne.
        builder.HasOne(e => e.Employer)
            .WithMany()
            .HasForeignKey(e => e.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

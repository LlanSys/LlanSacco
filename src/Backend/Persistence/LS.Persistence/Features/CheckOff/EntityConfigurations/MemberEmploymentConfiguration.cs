using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.EntityConfigurations;

internal sealed class MemberEmploymentConfiguration : IEntityTypeConfiguration<MemberEmployment>
{
    public void Configure(EntityTypeBuilder<MemberEmployment> builder)
    {
        

        builder.ToTable("MemberEmployments", "checkoff");

        builder.Property(e => e.EmployeePayrollNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => new { e.TenantId, e.EmployerId, e.EmployeePayrollNumber }).IsUnique();

        // MemberId references Membership by identity only. A navigation here would make CheckOff
        // migrations create a second copy of the Membership tables.
        builder.HasIndex(e => e.MemberId);

        builder.HasOne(e => e.Employer)
            .WithMany()
            .HasForeignKey(e => e.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

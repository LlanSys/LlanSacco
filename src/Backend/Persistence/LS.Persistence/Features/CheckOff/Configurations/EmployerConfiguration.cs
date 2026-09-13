using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class EmployerConfiguration : IEntityTypeConfiguration<Employer>
{
    public void Configure(EntityTypeBuilder<Employer> builder)
    {
        

        builder.ToTable("Employers", "checkoff");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ContactPerson)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Email)
            .HasMaxLength(150);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(50);
            
        builder.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
    }
}

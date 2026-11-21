using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries");

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Relationship).IsRequired().HasMaxLength(50);
        builder.Property(x => x.IdentificationNumber).HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.AllocationPercentage).HasPrecision(5, 2);

        builder.HasIndex(x => new { x.TenantId, x.MemberId });
    }
}

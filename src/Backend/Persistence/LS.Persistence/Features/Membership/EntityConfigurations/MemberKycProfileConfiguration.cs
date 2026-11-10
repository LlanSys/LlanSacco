using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class MemberKycProfileConfiguration : IEntityTypeConfiguration<MemberKycProfile>
{
    public void Configure(EntityTypeBuilder<MemberKycProfile> builder)
    {
        builder.ToTable("MemberKycProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VerificationNotes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.TenantId, x.MemberId }).IsUnique();
    }
}

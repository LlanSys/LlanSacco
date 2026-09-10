using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class MemberGuarantorConfiguration : IEntityTypeConfiguration<MemberGuarantor>
{
    public void Configure(EntityTypeBuilder<MemberGuarantor> builder)
    {
        builder.ToTable("MemberGuarantors");

        builder.Property(x => x.GuaranteedAmount).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.TenantId, x.MemberId });
        builder.HasIndex(x => new { x.TenantId, x.GuarantorMemberId });

        builder.HasOne(x => x.Member)
               .WithMany()
               .HasForeignKey(x => x.MemberId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Guarantor)
               .WithMany()
               .HasForeignKey(x => x.GuarantorMemberId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

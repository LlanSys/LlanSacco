using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class GuarantorRequestConfiguration : IEntityTypeConfiguration<GuarantorRequest>
{
    public void Configure(EntityTypeBuilder<GuarantorRequest> builder)
    {
        builder.ToTable("GuarantorRequests");

        builder.Property(x => x.AmountToGuarantee).HasPrecision(18, 4);
        builder.Property(x => x.ResponseNote).HasMaxLength(500);

        builder.HasIndex(x => new { x.TenantId, x.MemberId });
        builder.HasIndex(x => new { x.TenantId, x.NominatedGuarantorMemberId });

        builder.HasOne(x => x.Member)
               .WithMany()
               .HasForeignKey(x => x.MemberId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NominatedGuarantor)
               .WithMany()
               .HasForeignKey(x => x.NominatedGuarantorMemberId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class MemberTransactionConfiguration : IEntityTypeConfiguration<MemberTransaction>
{
    public void Configure(EntityTypeBuilder<MemberTransaction> builder)
    {
        builder.ToTable("MemberTransactions");

        builder.Property(x => x.Reference).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Amount).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.TenantId, x.MemberAccountId });
        builder.HasIndex(x => new { x.TenantId, x.Reference }).IsUnique();

        builder.HasOne(x => x.Account)
               .WithMany()
               .HasForeignKey(x => x.MemberAccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Member)
               .WithMany()
               .HasForeignKey(x => x.MemberId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

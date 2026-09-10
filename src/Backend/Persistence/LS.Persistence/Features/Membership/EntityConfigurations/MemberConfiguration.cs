using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.Property(x => x.MemberNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.IdentificationNumber).HasMaxLength(100);

        builder.HasIndex(x => new { x.TenantId, x.MemberNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.IdentificationNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.AppUserId }).IsUnique().HasFilter("[AppUserId] IS NOT NULL");
        
        builder.HasOne(x => x.KycProfile)
               .WithOne(x => x.Member)
               .HasForeignKey<MemberKycProfile>(x => x.MemberId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Accounts)
               .WithOne(x => x.Member)
               .HasForeignKey(x => x.MemberId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Beneficiaries)
               .WithOne(x => x.Member)
               .HasForeignKey(x => x.MemberId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

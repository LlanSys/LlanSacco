using LS.Domain.Features.Membership.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Membership.EntityConfigurations;

internal sealed class MemberAccountConfiguration : IEntityTypeConfiguration<MemberAccount>
{
    public void Configure(EntityTypeBuilder<MemberAccount> builder)
    {
        builder.ToTable("MemberAccounts");

        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CurrentBalance).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.TenantId, x.AccountNumber }).IsUnique();
    }
}

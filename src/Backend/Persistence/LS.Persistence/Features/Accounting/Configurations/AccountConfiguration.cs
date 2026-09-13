using LS.Domain.Features.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Accounting.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", "accounting");

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.AccountCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => new { e.TenantId, e.AccountCode }).IsUnique();

        builder.HasData(LS.Persistence.Features.Accounting.Seeds.ChartOfAccountsSeed.Accounts);

    }
}

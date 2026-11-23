using LS.Domain.Features.Dividends.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Dividends.EntityConfigurations;

internal sealed class DividendDistributionPreferenceConfiguration : IEntityTypeConfiguration<DividendDistributionPreference>
{
    public void Configure(EntityTypeBuilder<DividendDistributionPreference> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.MemberId }).IsUnique();

        builder.Property(x => x.CapitalizePercentage).HasColumnType("decimal(5,2)");
        builder.Property(x => x.FosaPercentage).HasColumnType("decimal(5,2)");
        builder.Property(x => x.ExternalBankPercentage).HasColumnType("decimal(5,2)");
    }
}

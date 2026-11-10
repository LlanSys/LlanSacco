using LS.Domain.Features.Dividends.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Dividends.Configurations;

public class DividendCalculationConfiguration : IEntityTypeConfiguration<DividendCalculation>
{
    public void Configure(EntityTypeBuilder<DividendCalculation> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => new { x.DeclarationId, x.MemberId }).IsUnique();

        builder.Property(x => x.WeightedShareBalance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrossShareDividend).HasColumnType("decimal(18,2)");
        builder.Property(x => x.WeightedDepositBalance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrossDepositInterest).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShareWithholdingTax).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DepositWithholdingTax).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalWithholdingTax).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NetPayout).HasColumnType("decimal(18,2)");
    }
}

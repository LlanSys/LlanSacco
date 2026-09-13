using LS.Domain.Features.Banking.Deposits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class DepositProductConfiguration : IEntityTypeConfiguration<DepositProduct>
{
    public void Configure(EntityTypeBuilder<DepositProduct> builder)
    {
        builder.ToTable("DepositProducts", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.InterestRate).HasPrecision(18, 4);
        builder.Property(x => x.MinimumDeposit).HasPrecision(18, 4);
        builder.Property(x => x.FlatPenaltyRate).HasPrecision(18, 4);
        builder.Property(x => x.InterestForfeiturePercentage).HasPrecision(18, 4);
        builder.Property(x => x.ProRataReducedInterestRate).HasPrecision(18, 4);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

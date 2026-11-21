using LS.Domain.Features.Banking.Savings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class SavingsProductConfiguration : IEntityTypeConfiguration<SavingsProduct>
{
    public void Configure(EntityTypeBuilder<SavingsProduct> builder)
    {
        builder.ToTable("SavingsProducts", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.InterestRate).HasPrecision(18, 4);
        builder.Property(x => x.MinimumBalance).HasPrecision(18, 4);
        builder.Property(x => x.WithdrawalFee).HasPrecision(18, 4);
        
        builder.Property(x => x.DailyWithdrawalLimit).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyWithdrawalLimit).HasPrecision(18, 4);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

using LS.Domain.Features.Banking.Deposits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class DepositTransactionConfiguration : IEntityTypeConfiguration<DepositTransaction>
{
    public void Configure(EntityTypeBuilder<DepositTransaction> builder)
    {
        builder.ToTable("DepositTransactions", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 4);
        builder.Property(x => x.Reference).HasMaxLength(100);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.DepositAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

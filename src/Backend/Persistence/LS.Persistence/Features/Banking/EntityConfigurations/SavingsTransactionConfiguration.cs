using LS.Domain.Features.Banking.Savings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class SavingsTransactionConfiguration : IEntityTypeConfiguration<SavingsTransaction>
{
    public void Configure(EntityTypeBuilder<SavingsTransaction> builder)
    {
        builder.ToTable("SavingsTransactions", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.ExternalReferenceId).HasMaxLength(100);
        
        builder.HasIndex(x => x.SavingsAccountId);
        builder.HasIndex(x => x.ExternalReferenceId); // useful for checking if a webhook already processed
    }
}

using LS.Domain.Features.Banking.Savings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
{
    public void Configure(EntityTypeBuilder<SavingsAccount> builder)
    {
        builder.ToTable("SavingsAccounts", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Balance).HasPrecision(18, 4);
        builder.Property(x => x.LockedFunds).HasPrecision(18, 4);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.SavingsProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MemberId, x.SavingsProductId }).IsUnique();
    }
}

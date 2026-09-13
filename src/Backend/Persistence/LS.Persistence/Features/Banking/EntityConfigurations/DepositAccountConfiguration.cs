using LS.Domain.Features.Banking.Deposits.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

public class DepositAccountConfiguration : IEntityTypeConfiguration<DepositAccount>
{
    public void Configure(EntityTypeBuilder<DepositAccount> builder)
    {
        builder.ToTable("DepositAccounts", "banking");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Balance).HasPrecision(18, 4);
        builder.Property(x => x.AccruedInterest).HasPrecision(18, 4);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.DepositProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MemberId, x.DepositProductId });
    }
}

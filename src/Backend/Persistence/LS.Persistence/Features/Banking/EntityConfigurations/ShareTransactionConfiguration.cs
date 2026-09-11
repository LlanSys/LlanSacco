using LS.Domain.Features.Banking.Shares.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

internal sealed class ShareTransactionConfiguration : IEntityTypeConfiguration<ShareTransaction>
{
    public void Configure(EntityTypeBuilder<ShareTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.ShareAccountId);
        builder.HasIndex(x => x.CounterpartyAccountId);

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.Property(x => x.PricePerShare)
            .HasPrecision(18, 2);
            
        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);
            
        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.ShareAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

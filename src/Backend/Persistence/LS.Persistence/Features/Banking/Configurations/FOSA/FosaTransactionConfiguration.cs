using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.Configurations.FOSA;

public class FosaTransactionConfiguration : IEntityTypeConfiguration<FosaTransaction>
{
    public void Configure(EntityTypeBuilder<FosaTransaction> builder)
    {
        builder.ToTable("FosaTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 4);

        builder.Property(x => x.Reference)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.FosaAccount)
            .WithMany()
            .HasForeignKey(x => x.FosaAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TellerTill)
            .WithMany()
            .HasForeignKey(x => x.TellerTillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Reference);
    }
}

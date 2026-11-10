using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.Configurations.FOSA;

public class TillBalancingRecordConfiguration : IEntityTypeConfiguration<TillBalancingRecord>
{
    public void Configure(EntityTypeBuilder<TillBalancingRecord> builder)
    {
        builder.ToTable("TillBalancingRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SystemBalance)
            .HasPrecision(18, 4);

        builder.Property(x => x.PhysicalCount)
            .HasPrecision(18, 4);

        builder.Property(x => x.Remarks)
            .HasMaxLength(500);

        builder.HasOne(x => x.TellerTill)
            .WithMany()
            .HasForeignKey(x => x.TellerTillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.Breakdown, breakdown =>
        {
            breakdown.Property(b => b.Note1000Count).HasColumnName("Note1000Count");
            breakdown.Property(b => b.Note500Count).HasColumnName("Note500Count");
            breakdown.Property(b => b.Note200Count).HasColumnName("Note200Count");
            breakdown.Property(b => b.Note100Count).HasColumnName("Note100Count");
            breakdown.Property(b => b.Note50Count).HasColumnName("Note50Count");
            breakdown.Property(b => b.Coin20Count).HasColumnName("Coin20Count");
            breakdown.Property(b => b.Coin10Count).HasColumnName("Coin10Count");
            breakdown.Property(b => b.Coin5Count).HasColumnName("Coin5Count");
            breakdown.Property(b => b.Coin1Count).HasColumnName("Coin1Count");
        });
    }
}

using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.Configurations.FOSA;

public class TellerTillConfiguration : IEntityTypeConfiguration<TellerTill>
{
    public void Configure(EntityTypeBuilder<TellerTill> builder)
    {
        builder.ToTable("TellerTills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TillName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 4);
            // .IsConcurrencyToken(); wait, BaseEntity already provides a RowVersion for concurrency control!

        builder.Property(x => x.MaxLimit)
            .HasPrecision(18, 4);

        builder.HasIndex(x => x.TillName)
            .IsUnique();
    }
}

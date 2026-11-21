using LS.Domain.Features.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Accounting.Configurations;

public class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.ToTable("JournalLines", "accounting");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Debit).HasPrecision(18, 4);
        builder.Property(e => e.Credit).HasPrecision(18, 4);

        builder.HasOne(e => e.Account)
            .WithMany(e => e.JournalLines)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.BranchId).IsRequired(false);
        builder.Property(e => e.CostCenterId).IsRequired(false);

        builder.HasData(LS.Persistence.Features.Accounting.Seeds.ChartOfAccountsSeed.JournalLines);
    }
}

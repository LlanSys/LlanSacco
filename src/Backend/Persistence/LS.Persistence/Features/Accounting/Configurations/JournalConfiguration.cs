using LS.Domain.Features.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Accounting.Configurations;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.ToTable("Journals", "accounting");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReferenceNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(e => new { e.TenantId, e.ReferenceNumber }).IsUnique();

        builder.HasData(LS.Persistence.Features.Accounting.Seeds.ChartOfAccountsSeed.Journals);

        builder.HasMany(e => e.Lines)
            .WithOne(e => e.Journal)
            .HasForeignKey(e => e.JournalId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}

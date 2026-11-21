using LS.Domain.Features.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Accounting.EntityConfigurations;

public class TransactionTypeGlMappingConfiguration : IEntityTypeConfiguration<TransactionTypeGlMapping>
{
    public void Configure(EntityTypeBuilder<TransactionTypeGlMapping> builder)
    {
        builder.ToTable("TransactionTypeGlMappings", "accounting");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TransactionTypeCode)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.DebitSideSource).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreditSideSource).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FeeSideSource).IsRequired().HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.HasOne(x => x.DebitAccount)
            .WithMany()
            .HasForeignKey(x => x.DebitExplicitGlAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreditAccount)
            .WithMany()
            .HasForeignKey(x => x.CreditExplicitGlAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FeeAccount)
            .WithMany()
            .HasForeignKey(x => x.FeeExplicitGlAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // A tenant should only have one mapping per transaction type
        builder.HasIndex(x => new { x.TenantId, x.TransactionTypeCode })
            .IsUnique();
            
        builder.HasData(LS.Persistence.Features.Accounting.Seeds.ChartOfAccountsSeed.TransactionTypeGlMappings);
    }
}

using LS.Domain.Features.Dividends.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Dividends.Configurations;

public class DividendDeclarationConfiguration : IEntityTypeConfiguration<DividendDeclaration>
{
    public void Configure(EntityTypeBuilder<DividendDeclaration> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.FinancialYear).IsUnique();

        builder.Property(x => x.ShareDividendRate).HasColumnType("decimal(5,4)");
        builder.Property(x => x.DepositInterestRate).HasColumnType("decimal(5,4)");
        builder.Property(x => x.ShareWhtRate).HasColumnType("decimal(5,4)");
        builder.Property(x => x.DepositWhtRate).HasColumnType("decimal(5,4)");
        
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(x => x.Calculations)
            .WithOne(x => x.Declaration)
            .HasForeignKey(x => x.DeclarationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

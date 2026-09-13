using LS.Domain.Features.Banking.Shares.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

internal sealed class ShareProductConfiguration : IEntityTypeConfiguration<ShareProduct>
{
    public void Configure(EntityTypeBuilder<ShareProduct> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.PricePerShare)
            .HasPrecision(18, 2);
    }
}

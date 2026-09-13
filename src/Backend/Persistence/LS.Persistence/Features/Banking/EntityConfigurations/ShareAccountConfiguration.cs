using LS.Domain.Features.Banking.Shares.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.EntityConfigurations;

internal sealed class ShareAccountConfiguration : IEntityTypeConfiguration<ShareAccount>
{
    public void Configure(EntityTypeBuilder<ShareAccount> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.MemberId);
        builder.HasIndex(x => x.ShareProductId);

        builder.Property(x => x.TotalValue)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ShareProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Ensure a member only has one share account per share product
        builder.HasIndex(x => new { x.TenantId, x.MemberId, x.ShareProductId }).IsUnique();
    }
}

using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.FOSA.EntityConfigurations;

internal sealed class VaultTransferConfiguration : IEntityTypeConfiguration<VaultTransfer>
{
    public void Configure(EntityTypeBuilder<VaultTransfer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("VaultTransfers");
        builder.HasKey(transfer => new { transfer.TenantId, transfer.Id });
        builder.Property(transfer => transfer.Amount).HasPrecision(18, 4);
        builder.Property(transfer => transfer.Denominations).HasMaxLength(512).IsRequired();
        builder.Property(transfer => transfer.Remarks).HasMaxLength(500).IsRequired();
        builder.HasOne<TellerTill>().WithMany().HasForeignKey(transfer => transfer.TellerTillId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Vault>().WithMany().HasForeignKey(transfer => transfer.VaultId).OnDelete(DeleteBehavior.Restrict);
    }
}

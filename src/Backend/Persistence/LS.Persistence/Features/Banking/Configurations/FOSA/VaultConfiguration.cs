using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.Configurations.FOSA;

public class VaultConfiguration : IEntityTypeConfiguration<Vault>
{
    public void Configure(EntityTypeBuilder<Vault> builder)
    {
        builder.ToTable("Vaults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 4);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}

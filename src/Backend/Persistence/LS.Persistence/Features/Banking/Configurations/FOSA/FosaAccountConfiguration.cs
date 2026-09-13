using LS.Domain.Features.Banking.FOSA.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.Configurations.FOSA;

public class FosaAccountConfiguration : IEntityTypeConfiguration<FosaAccount>
{
    public void Configure(EntityTypeBuilder<FosaAccount> builder)
    {
        builder.ToTable("FosaAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Balance)
            .HasPrecision(18, 4);

        builder.Property(x => x.OverdraftLimit)
            .HasPrecision(18, 4);

        builder.HasIndex(x => x.AccountNumber)
            .IsUnique();
            
        builder.HasIndex(x => x.MemberId);
    }
}

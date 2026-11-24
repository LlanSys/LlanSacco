using LS.Domain.Features.Loans.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

internal sealed class LoanProductConfiguration : IEntityTypeConfiguration<LoanProduct>
{
    public void Configure(EntityTypeBuilder<LoanProduct> builder)
    {
        builder.Property(x => x.GuarantorPolicyVersion).HasMaxLength(80).HasDefaultValue("LLANSACCO-REFERENCE/0.1");
        builder.Property(x => x.GuarantorReservationDays).HasDefaultValue(30);
        builder.Property(x => x.GuarantorConsentDays).HasDefaultValue(14);
        builder.Property(x => x.EligibleDepositMultiple).HasPrecision(9, 4).HasDefaultValue(3m);
        builder.ToTable("LoanProducts");

        builder.Property(x => x.ProductCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.InterestRate).HasPrecision(18, 4);
        builder.Property(x => x.MaxAmount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.TenantId, x.ProductCode }).IsUnique();
    }
}

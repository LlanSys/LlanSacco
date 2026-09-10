using LS.Domain.Features.Loans.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

internal sealed class LoanRepaymentConfiguration : IEntityTypeConfiguration<LoanRepayment>
{
    public void Configure(EntityTypeBuilder<LoanRepayment> builder)
    {
        builder.ToTable("LoanRepayments");

        builder.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PrincipalComponent).HasPrecision(18, 2);
        builder.Property(x => x.InterestComponent).HasPrecision(18, 2);
        builder.Property(x => x.PenaltyComponent).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.TenantId, x.ReceiptNumber }).IsUnique();
    }
}

using LS.Domain.Features.Loans.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

internal sealed class LoanGuarantorConfiguration : IEntityTypeConfiguration<LoanGuarantor>
{
    public void Configure(EntityTypeBuilder<LoanGuarantor> builder)
    {
        builder.ToTable("LoanGuarantors");

        builder.Property(x => x.GuaranteedAmount).HasPrecision(18, 2);
        builder.Property(x => x.LockedAmount).HasPrecision(18, 2);
    }
}

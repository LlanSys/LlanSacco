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
        builder.Property(x => x.AcceptedAmount).HasPrecision(18, 2);
        builder.Property(x => x.ReservationPolicyVersion).HasMaxLength(80);
        builder.Property(x => x.ReservationFailure).HasMaxLength(300);
        builder.Property(x => x.ConsentActorId).HasMaxLength(200);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.LoanApplicationId, x.GuarantorMemberId });
    }
}

using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.GuarantorReservations.EntityConfigurations;

internal sealed class GuarantorReservationConfiguration : IEntityTypeConfiguration<GuarantorReservation>
{
    public void Configure(EntityTypeBuilder<GuarantorReservation> builder)
    {
        builder.ToTable("GuarantorReservations", "banking");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.PledgeId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.LoanId });
        builder.HasIndex(x => new { x.TenantId, x.SavingsAccountId });
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.RemainingAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.PolicyVersion).HasMaxLength(80);
        builder.Property(x => x.DecisionReason).HasMaxLength(300);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Banking.GuarantorReservations.EntityConfigurations;

internal sealed class GuarantorReservationReceiptConfiguration : IEntityTypeConfiguration<GuarantorReservationReceipt>
{
    public void Configure(EntityTypeBuilder<GuarantorReservationReceipt> builder)
    {
        builder.ToTable("GuarantorReservationReceipts", "banking");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.OperationId }).IsUnique();
        builder.Property(x => x.Fingerprint).HasMaxLength(64);
        builder.Property(x => x.OutcomeJson).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

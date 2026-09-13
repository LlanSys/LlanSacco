using LS.Domain.Features.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Accounting.EntityConfigurations;

public class AccountingIntegrationErrorConfiguration : IEntityTypeConfiguration<AccountingIntegrationError>
{
    public void Configure(EntityTypeBuilder<AccountingIntegrationError> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventPayload)
            .IsRequired(); // No max length, it's a JSON blob

        builder.Property(e => e.ErrorMessage)
            .IsRequired(); // No max length

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        // Standard audits are handled by convention/base class
    }
}

using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class CheckoffBatchConfiguration : IEntityTypeConfiguration<CheckoffBatch>
{
    public void Configure(EntityTypeBuilder<CheckoffBatch> builder)
    {
        

        builder.ToTable("CheckoffBatches", "checkoff");

        builder.Property(e => e.TotalAmountReceived)
            .HasPrecision(18, 4);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(e => e.Employer)
            .WithMany()
            .HasForeignKey(e => e.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

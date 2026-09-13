using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class CheckoffStagingRowConfiguration : IEntityTypeConfiguration<CheckoffStagingRow>
{
    public void Configure(EntityTypeBuilder<CheckoffStagingRow> builder)
    {
        

        builder.ToTable("CheckoffStagingRows", "checkoff");

        builder.Property(e => e.RawPayrollNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.RawMemberName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.AmountForSavings).HasPrecision(18, 4);
        builder.Property(e => e.AmountForShares).HasPrecision(18, 4);
        builder.Property(e => e.AmountForLoans).HasPrecision(18, 4);
        builder.Property(e => e.TotalDeducted).HasPrecision(18, 4);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(e => e.ExceptionReason)
            .HasMaxLength(500);

        builder.HasOne(e => e.Batch)
            .WithMany(b => b.StagingRows)
            .HasForeignKey(e => e.CheckoffBatchId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.ResolvedMember)
            .WithMany()
            .HasForeignKey(e => e.ResolvedMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

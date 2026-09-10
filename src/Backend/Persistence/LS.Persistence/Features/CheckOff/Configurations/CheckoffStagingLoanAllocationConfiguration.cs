using LS.Domain.Features.CheckOff.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class CheckoffStagingLoanAllocationConfiguration : IEntityTypeConfiguration<CheckoffStagingLoanAllocation>
{
    public void Configure(EntityTypeBuilder<CheckoffStagingLoanAllocation> builder)
    {
        builder.ToTable("CheckoffStagingLoanAllocations", "checkoff");

        builder.Property(e => e.Amount).HasPrecision(18, 4);

        builder.HasOne(e => e.StagingRow)
            .WithMany(r => r.LoanAllocations)
            .HasForeignKey(e => e.CheckoffStagingRowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

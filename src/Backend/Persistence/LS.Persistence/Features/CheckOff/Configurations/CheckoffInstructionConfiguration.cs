using LS.Domain.Features.CheckOff.Entities;
using LS.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.CheckOff.Configurations;

public class CheckoffInstructionConfiguration : IEntityTypeConfiguration<CheckoffInstruction>
{
    public void Configure(EntityTypeBuilder<CheckoffInstruction> builder)
    {
        

        builder.ToTable("CheckoffInstructions", "checkoff");

        builder.Property(e => e.ExpectedSavingsAmount)
            .HasPrecision(18, 4);

        builder.Property(e => e.ExpectedSharesAmount)
            .HasPrecision(18, 4);

        builder.HasOne(e => e.MemberEmployment)
            .WithMany()
            .HasForeignKey(e => e.MemberEmploymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

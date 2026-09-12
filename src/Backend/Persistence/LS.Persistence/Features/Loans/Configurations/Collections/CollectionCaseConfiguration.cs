using LS.Domain.Features.Loans.Collections.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LS.Persistence.Common;

namespace LS.Persistence.Features.Loans.Configurations.Collections;

public class CollectionCaseConfiguration : IEntityTypeConfiguration<CollectionCase>
{
    public void Configure(EntityTypeBuilder<CollectionCase> builder)
    {
        builder.ToTable("CollectionCases");

        builder.Property(c => c.TotalArrearsAmount).HasPrecision(18, 2);
        builder.Property(c => c.ResolutionReason).HasMaxLength(500);

        builder.HasOne(c => c.LoanApplication)
               .WithMany()
               .HasForeignKey(c => c.LoanApplicationId)
               .OnDelete(DeleteBehavior.Restrict);
               
        // Enforce only one OPEN collection case per loan application.
        // Assuming CollectionCaseStatus.Open = 1
        builder.HasIndex(c => c.LoanApplicationId)
               .IsUnique()
               .HasFilter("[Status] = 1 AND IsDeleted = 0");
    }
}

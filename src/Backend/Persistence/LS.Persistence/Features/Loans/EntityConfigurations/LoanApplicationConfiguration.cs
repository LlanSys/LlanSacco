using LS.Domain.Features.Loans.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

internal sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications");

        builder.Property(x => x.ApplicationNumber).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(x => x.InterestRate).HasPrecision(18, 4);
        builder.Property(x => x.OutstandingPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.OutstandingInterest).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.TenantId, x.ApplicationNumber }).IsUnique();
        
        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.LoanProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Guarantors)
               .WithOne(x => x.Loan)
               .HasForeignKey(x => x.LoanApplicationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Repayments)
               .WithOne(x => x.Loan)
               .HasForeignKey(x => x.LoanApplicationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

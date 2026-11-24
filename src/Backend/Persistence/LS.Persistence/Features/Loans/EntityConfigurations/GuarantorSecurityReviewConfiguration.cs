using LS.Domain.Features.Loans.Guarantors.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

internal sealed class GuarantorSecurityReviewConfiguration : IEntityTypeConfiguration<GuarantorSecurityReview>
{
    public void Configure(EntityTypeBuilder<GuarantorSecurityReview> builder)
    {
        builder.ToTable("GuarantorSecurityReviews", "loans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasMaxLength(30);
        builder.Property(x => x.SourceHeldAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasMaxLength(30);
        builder.Property(x => x.EvidenceReference).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(300);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.LoanId });
    }
}

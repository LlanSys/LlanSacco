using LS.Domain.Features.Loans.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LS.Persistence.Features.Loans.EntityConfigurations;

public class LoanRepaymentScheduleConfiguration : IEntityTypeConfiguration<LoanRepaymentSchedule>
{
    public void Configure(EntityTypeBuilder<LoanRepaymentSchedule> builder)
    {
        builder.ToTable("LoanRepaymentSchedules");

        builder.Property(x => x.PrincipalExpected)
            .HasPrecision(18, 2);

        builder.Property(x => x.InterestExpected)
            .HasPrecision(18, 2);

        builder.Property(x => x.AmountPaid)
            .HasPrecision(18, 2);
            
        builder.Ignore(x => x.TotalExpected);
    }
}

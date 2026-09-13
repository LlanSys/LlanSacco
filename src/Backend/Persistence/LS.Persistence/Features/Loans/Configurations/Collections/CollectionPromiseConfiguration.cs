using LS.Domain.Features.Loans.Collections.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LS.Persistence.Common;

namespace LS.Persistence.Features.Loans.Configurations.Collections;

public class CollectionPromiseConfiguration : IEntityTypeConfiguration<CollectionPromise>
{
    public void Configure(EntityTypeBuilder<CollectionPromise> builder)
    {
        builder.ToTable("CollectionPromises");

        builder.Property(p => p.PromiseAmount).HasPrecision(18, 2);

        builder.HasOne(p => p.CollectionCase)
               .WithMany(c => c.Promises)
               .HasForeignKey(p => p.CollectionCaseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

using LS.Domain.Features.Loans.Collections.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LS.Persistence.Common;

namespace LS.Persistence.Features.Loans.Configurations.Collections;

public class CollectionActionConfiguration : IEntityTypeConfiguration<CollectionAction>
{
    public void Configure(EntityTypeBuilder<CollectionAction> builder)
    {
        builder.ToTable("CollectionActions");

        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasOne(a => a.CollectionCase)
               .WithMany(c => c.Actions)
               .HasForeignKey(a => a.CollectionCaseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

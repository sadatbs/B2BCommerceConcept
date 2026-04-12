using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class RequisitionConfiguration : IEntityTypeConfiguration<Requisition>
{
    public void Configure(EntityTypeBuilder<Requisition> builder)
    {
        builder.ToTable("Requisitions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.CustomerId).IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.SubmittedAt).IsRequired();
        builder.Property(r => r.ResolvedAt);

        builder.HasMany(r => r.LineItems)
            .WithOne(li => li.Requisition)
            .HasForeignKey(li => li.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.SubmittedAt);

        builder.Ignore(r => r.DomainEvents);

        builder.Navigation(r => r.LineItems)
            .HasField("_lineItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

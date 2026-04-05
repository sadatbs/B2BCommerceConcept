using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.IssuedAt)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt);

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.DueDate);
    }
}

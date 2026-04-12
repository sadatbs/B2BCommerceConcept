using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class RequisitionLineItemConfiguration : IEntityTypeConfiguration<RequisitionLineItem>
{
    public void Configure(EntityTypeBuilder<RequisitionLineItem> builder)
    {
        builder.ToTable("RequisitionLineItems");

        builder.HasKey(li => li.Id);

        builder.Property(li => li.ProductId).IsRequired();

        // Intentionally no FK to Products table — snapshot data
        builder.Property(li => li.Sku)
            .HasMaxLength(50).IsRequired();

        builder.Property(li => li.ProductName)
            .HasMaxLength(200).IsRequired();

        builder.Property(li => li.UnitPrice)
            .HasPrecision(18, 2).IsRequired();

        builder.Property(li => li.Quantity).IsRequired();

        builder.HasIndex(li => li.RequisitionId);
        // No index on ProductId — snapshots are read with their parent Requisition
    }
}

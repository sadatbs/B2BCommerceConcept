using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // No FK to Product — snapshot is intentional
        builder.HasIndex(oi => oi.OrderId);
    }
}

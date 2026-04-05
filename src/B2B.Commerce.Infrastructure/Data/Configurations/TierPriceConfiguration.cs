using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class TierPriceConfiguration : IEntityTypeConfiguration<TierPrice>
{
    public void Configure(EntityTypeBuilder<TierPrice> builder)
    {
        builder.ToTable("TierPrices");

        builder.HasKey(tp => new { tp.PriceTierId, tp.ProductId });

        builder.Property(tp => tp.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(tp => tp.CreatedAt)
            .IsRequired();

        builder.HasOne(tp => tp.Product)
            .WithMany()
            .HasForeignKey(tp => tp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tp => tp.ProductId);
    }
}

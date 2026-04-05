using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class PriceTierConfiguration : IEntityTypeConfiguration<PriceTier>
{
    public void Configure(EntityTypeBuilder<PriceTier> builder)
    {
        builder.ToTable("PriceTiers");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pt => pt.Description)
            .HasMaxLength(500);

        builder.Property(pt => pt.CreatedAt)
            .IsRequired();

        builder.HasMany(pt => pt.Prices)
            .WithOne(tp => tp.PriceTier)
            .HasForeignKey(tp => tp.PriceTierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pt => pt.Name)
            .IsUnique();

        builder.Navigation(pt => pt.Prices)
            .HasField("_prices")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

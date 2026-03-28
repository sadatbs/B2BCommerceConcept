using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerId);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Items property returns _items.AsReadOnly() — EF cannot write to a ReadOnlyCollection.
        // UsePropertyAccessMode(Field) forces EF to always use _items directly for all
        // access (loading, fixup, change detection), bypassing the read-only wrapper.
        builder.Navigation(c => c.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.CustomerId)
            .HasFilter("\"CustomerId\" IS NOT NULL");

        builder.Ignore(c => c.DomainEvents);
    }
}

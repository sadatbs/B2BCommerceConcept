using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2B.Commerce.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(c => c.PaymentTerms)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasOne(c => c.PriceTier)
            .WithMany()
            .HasForeignKey(c => c.PriceTierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Users)
            .WithOne(u => u.Customer)
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.Email);

        builder.Navigation(c => c.Users)
            .HasField("_users")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

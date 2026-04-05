using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Data;

public class CommerceDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PriceTier> PriceTiers => Set<PriceTier>();
    public DbSet<TierPrice> TierPrices => Set<TierPrice>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public CommerceDbContext(DbContextOptions<CommerceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing inline config (Products, Catalogs, CatalogProducts)
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Sku).HasMaxLength(50).IsRequired();
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Category).HasMaxLength(100);
        });

        modelBuilder.Entity<Catalog>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CatalogProduct>(entity =>
        {
            entity.HasKey(cp => new { cp.CatalogId, cp.ProductId });

            entity.HasOne(cp => cp.Catalog)
                  .WithMany(c => c.CatalogProducts)
                  .HasForeignKey(cp => cp.CatalogId);

            entity.HasOne(cp => cp.Product)
                  .WithMany(p => p.CatalogProducts)
                  .HasForeignKey(cp => cp.ProductId);
        });

        // New configuration classes
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PriceTierConfiguration());
        modelBuilder.ApplyConfiguration(new TierPriceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
    }
}

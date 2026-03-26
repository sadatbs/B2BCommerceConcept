using B2B.Commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Data;

public class CommerceDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();

    public CommerceDbContext(DbContextOptions<CommerceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}

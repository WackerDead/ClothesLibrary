using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class ClothingDbContext : DbContext
{
    public ClothingDbContext(DbContextOptions<ClothingDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Outfit> Outfits { get; set; }
    
    //TODO: add onmodelcreating to use fluent api
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductColor>()
            .HasKey(pc => new { pc.ProductId, pc.ExactColor });
        modelBuilder.Entity<Product>().HasMany(p => p.ProductColors).WithOne(pc => pc.Product)
            .HasForeignKey(pc => pc.ProductId);
        modelBuilder.Entity<Color>().HasMany(c => c.ProductColors).WithOne(pc => pc.Color).HasForeignKey(pc => pc.ColorId);
        modelBuilder.Entity<Outfit>().HasMany(o => o.Clothes).WithMany();
    }
}
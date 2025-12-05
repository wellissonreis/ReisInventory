using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Infrastructure.Data;

public class SmartInventoryDbContext : DbContext
{
    public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockHistory> StockHistories => Set<StockHistory>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();
    public DbSet<PurchaseSuggestion> PurchaseSuggestions => Set<PurchaseSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(e => e.CurrentStock).HasColumnName("current_stock");
            entity.Property(e => e.SafetyStock).HasColumnName("safety_stock");
            entity.Property(e => e.LeadTimeDays).HasColumnName("lead_time_days");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // StockHistory configuration
        modelBuilder.Entity<StockHistory>(entity =>
        {
            entity.ToTable("stock_histories");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.QuantityChange).HasColumnName("quantity_change");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(100);

            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.ProductId, e.Date });

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockHistories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Forecast configuration
        modelBuilder.Entity<Forecast>(entity =>
        {
            entity.ToTable("forecasts");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.TargetDate).HasColumnName("target_date");
            entity.Property(e => e.PredictedDemand).HasColumnName("predicted_demand");
            entity.Property(e => e.StockOutRisk).HasColumnName("stock_out_risk").HasPrecision(5, 4);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.TargetDate);
            entity.HasIndex(e => new { e.ProductId, e.TargetDate });
            entity.HasIndex(e => e.StockOutRisk);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Forecasts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseSuggestion configuration
        modelBuilder.Entity<PurchaseSuggestion>(entity =>
        {
            entity.ToTable("purchase_suggestions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SuggestedQuantity).HasColumnName("suggested_quantity");
            entity.Property(e => e.Justification).HasColumnName("justification").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.PurchaseSuggestions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

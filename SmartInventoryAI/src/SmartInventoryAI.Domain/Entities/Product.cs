namespace SmartInventoryAI.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public int CurrentStock { get; private set; }
    public int SafetyStock { get; private set; }
    public int LeadTimeDays { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<StockHistory> StockHistories { get; private set; } = new List<StockHistory>();
    public ICollection<Forecast> Forecasts { get; private set; } = new List<Forecast>();
    public ICollection<PurchaseSuggestion> PurchaseSuggestions { get; private set; } = new List<PurchaseSuggestion>();

    private Product() { } // EF Core

    public static Product Create(
        string sku,
        string name,
        string category,
        int currentStock,
        int safetyStock,
        int leadTimeDays)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (safetyStock < 0)
            throw new ArgumentException("Safety stock cannot be negative", nameof(safetyStock));

        if (leadTimeDays < 0)
            throw new ArgumentException("Lead time cannot be negative", nameof(leadTimeDays));

        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = name,
            Category = category ?? string.Empty,
            CurrentStock = currentStock,
            SafetyStock = safetyStock,
            LeadTimeDays = leadTimeDays,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStock(int newStock)
    {
        CurrentStock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string category, int safetyStock, int leadTimeDays)
    {
        Name = name;
        Category = category;
        SafetyStock = safetyStock;
        LeadTimeDays = leadTimeDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLowStock() => CurrentStock <= SafetyStock;
}

namespace SmartInventoryAI.Domain.Entities;

public class StockHistory
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime Date { get; private set; }
    public int QuantityChange { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    // Navigation property
    public Product? Product { get; private set; }

    private StockHistory() { } // EF Core

    public static StockHistory Create(
        Guid productId,
        int quantityChange,
        string reason,
        DateTime? date = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty", nameof(reason));

        return new StockHistory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Date = date ?? DateTime.UtcNow,
            QuantityChange = quantityChange,
            Reason = reason
        };
    }

    public bool IsInbound() => QuantityChange > 0;
    public bool IsOutbound() => QuantityChange < 0;
}

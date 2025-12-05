namespace SmartInventoryAI.Domain.Entities;

public class Forecast
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime TargetDate { get; private set; }
    public int PredictedDemand { get; private set; }
    public decimal StockOutRisk { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Product? Product { get; private set; }

    private Forecast() { } // EF Core

    public static Forecast Create(
        Guid productId,
        DateTime targetDate,
        int predictedDemand,
        decimal stockOutRisk)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (predictedDemand < 0)
            throw new ArgumentException("Predicted demand cannot be negative", nameof(predictedDemand));

        if (stockOutRisk < 0 || stockOutRisk > 1)
            throw new ArgumentException("Stock out risk must be between 0 and 1", nameof(stockOutRisk));

        return new Forecast
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TargetDate = targetDate,
            PredictedDemand = predictedDemand,
            StockOutRisk = stockOutRisk,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsHighRisk() => StockOutRisk >= 0.7m;
    public bool IsMediumRisk() => StockOutRisk >= 0.4m && StockOutRisk < 0.7m;
    public bool IsLowRisk() => StockOutRisk < 0.4m;
}

namespace SmartInventoryAI.Domain.Entities;

public class PurchaseSuggestion
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int SuggestedQuantity { get; private set; }
    public string Justification { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Product? Product { get; private set; }

    private PurchaseSuggestion() { } // EF Core

    public static PurchaseSuggestion Create(
        Guid productId,
        int suggestedQuantity,
        string justification)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (suggestedQuantity <= 0)
            throw new ArgumentException("Suggested quantity must be positive", nameof(suggestedQuantity));

        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException("Justification cannot be empty", nameof(justification));

        return new PurchaseSuggestion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SuggestedQuantity = suggestedQuantity,
            Justification = justification,
            CreatedAt = DateTime.UtcNow
        };
    }
}

namespace SmartInventoryAI.Api.DTOs.Products;

public record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string Category,
    int CurrentStock,
    int SafetyStock,
    int LeadTimeDays,
    bool IsLowStock,
    DateTime CreatedAt,
    DateTime UpdatedAt);

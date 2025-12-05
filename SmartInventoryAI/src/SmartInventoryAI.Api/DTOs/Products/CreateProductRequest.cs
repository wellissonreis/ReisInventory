namespace SmartInventoryAI.Api.DTOs.Products;

public record CreateProductRequest(
    string Sku,
    string Name,
    string Category,
    int CurrentStock,
    int SafetyStock,
    int LeadTimeDays);

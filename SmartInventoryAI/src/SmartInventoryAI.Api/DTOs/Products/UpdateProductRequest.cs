namespace SmartInventoryAI.Api.DTOs.Products;

public record UpdateProductRequest(
    string Name,
    string Category,
    int SafetyStock,
    int LeadTimeDays);

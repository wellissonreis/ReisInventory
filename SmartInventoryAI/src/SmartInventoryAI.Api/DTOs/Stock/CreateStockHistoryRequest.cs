namespace SmartInventoryAI.Api.DTOs.Stock;

public record CreateStockHistoryRequest(
    Guid ProductId,
    int QuantityChange,
    string Reason,
    DateTime? Date = null);

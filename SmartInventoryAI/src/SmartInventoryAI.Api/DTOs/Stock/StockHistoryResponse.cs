namespace SmartInventoryAI.Api.DTOs.Stock;

public record StockHistoryResponse(
    Guid Id,
    Guid ProductId,
    DateTime Date,
    int QuantityChange,
    string Reason,
    string MovementType);

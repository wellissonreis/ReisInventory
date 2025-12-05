namespace SmartInventoryAI.Api.DTOs.Forecasts;

public record ForecastResponse(
    Guid Id,
    Guid ProductId,
    DateTime TargetDate,
    int PredictedDemand,
    decimal StockOutRisk,
    string RiskLevel,
    DateTime CreatedAt);

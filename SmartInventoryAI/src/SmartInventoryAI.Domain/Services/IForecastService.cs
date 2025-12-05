using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public interface IForecastService
{
    IEnumerable<Forecast> GenerateForecasts(
        Product product, 
        IEnumerable<StockHistory> stockHistory, 
        int forecastDays = 7);

    decimal CalculateStockOutRisk(
        int currentStock, 
        int safetyStock, 
        int predictedDemand, 
        int leadTimeDays);
}

using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public interface IForecastService
{
    /// <summary>
    /// Generates forecasts for a product based on its stock history.
    /// Uses a simple moving average algorithm for MVP.
    /// </summary>
    /// <param name="product">The product to forecast</param>
    /// <param name="stockHistory">Recent stock history entries</param>
    /// <param name="forecastDays">Number of days to forecast ahead</param>
    /// <returns>List of forecasts for the specified period</returns>
    IEnumerable<Forecast> GenerateForecasts(
        Product product, 
        IEnumerable<StockHistory> stockHistory, 
        int forecastDays = 7);

    /// <summary>
    /// Calculates the stock out risk based on current stock, safety stock, and predicted demand.
    /// </summary>
    /// <param name="currentStock">Current stock level</param>
    /// <param name="safetyStock">Safety stock level</param>
    /// <param name="predictedDemand">Predicted demand for the period</param>
    /// <param name="leadTimeDays">Lead time in days for replenishment</param>
    /// <returns>Risk value between 0 and 1</returns>
    decimal CalculateStockOutRisk(
        int currentStock, 
        int safetyStock, 
        int predictedDemand, 
        int leadTimeDays);
}

using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public class ForecastService : IForecastService
{
    private const int DefaultMovingAverageDays = 7;

    public IEnumerable<Forecast> GenerateForecasts(
        Product product, 
        IEnumerable<StockHistory> stockHistory, 
        int forecastDays = 7)
    {
        var historyList = stockHistory
            .Where(h => h.QuantityChange < 0)
            .OrderByDescending(h => h.Date)
            .Take(DefaultMovingAverageDays * 2)
            .ToList();

        if (historyList.Count == 0)
        {
            return GenerateZeroDemandForecasts(product.Id, forecastDays);
        }

        var dailyDemands = historyList
            .GroupBy(h => h.Date.Date)
            .Select(g => Math.Abs(g.Sum(h => h.QuantityChange)))
            .ToList();

        var averageDailyDemand = dailyDemands.Count > 0 
            ? (int)Math.Ceiling(dailyDemands.Average()) 
            : 0;

        var forecasts = new List<Forecast>();
        var today = DateTime.UtcNow.Date;

        for (int i = 1; i <= forecastDays; i++)
        {
            var targetDate = today.AddDays(i);
            var predictedDemand = averageDailyDemand;
            
            var cumulativeDemand = averageDailyDemand * i;
            var stockOutRisk = CalculateStockOutRisk(
                product.CurrentStock, 
                product.SafetyStock, 
                cumulativeDemand, 
                product.LeadTimeDays);

            var forecast = Forecast.Create(
                product.Id,
                targetDate,
                predictedDemand,
                stockOutRisk);

            forecasts.Add(forecast);
        }

        return forecasts;
    }

    public decimal CalculateStockOutRisk(
        int currentStock, 
        int safetyStock, 
        int predictedDemand, 
        int leadTimeDays)
    {
        if (currentStock <= 0)
            return 1.0m;

        var projectedStock = currentStock - predictedDemand;
        
        if (projectedStock <= 0)
            return 1.0m;

        if (projectedStock <= safetyStock)
        {
            var ratio = (decimal)projectedStock / safetyStock;
            return Math.Max(0, Math.Min(1, 1 - ratio + 0.3m));
        }

        var daysOfStock = predictedDemand > 0 
            ? (decimal)projectedStock / (predictedDemand > 0 ? predictedDemand : 1) 
            : 30;

        if (daysOfStock <= leadTimeDays)
        {
            return Math.Max(0.3m, Math.Min(0.7m, (decimal)leadTimeDays / daysOfStock * 0.3m));
        }

        return Math.Max(0, Math.Min(0.3m, (decimal)leadTimeDays / daysOfStock * 0.1m));
    }

    private static IEnumerable<Forecast> GenerateZeroDemandForecasts(Guid productId, int forecastDays)
    {
        var today = DateTime.UtcNow.Date;
        return Enumerable.Range(1, forecastDays)
            .Select(i => Forecast.Create(productId, today.AddDays(i), 0, 0))
            .ToList();
    }
}

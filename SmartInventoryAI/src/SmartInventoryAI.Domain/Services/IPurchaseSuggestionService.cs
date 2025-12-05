using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public interface IPurchaseSuggestionService
{
    PurchaseSuggestion? GenerateSuggestion(Product product, IEnumerable<Forecast> forecasts);

    int CalculateOrderQuantity(
        int averageDailyDemand, 
        int safetyStock, 
        int currentStock, 
        int leadTimeDays);
}

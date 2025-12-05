using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public interface IPurchaseSuggestionService
{
    /// <summary>
    /// Generates a purchase suggestion based on forecasts and current inventory levels.
    /// </summary>
    /// <param name="product">The product to analyze</param>
    /// <param name="forecasts">Recent forecasts for the product</param>
    /// <returns>A purchase suggestion if needed, null otherwise</returns>
    PurchaseSuggestion? GenerateSuggestion(Product product, IEnumerable<Forecast> forecasts);

    /// <summary>
    /// Calculates the recommended order quantity using EOQ (Economic Order Quantity) simplified.
    /// </summary>
    /// <param name="averageDailyDemand">Average daily demand</param>
    /// <param name="safetyStock">Safety stock level</param>
    /// <param name="currentStock">Current stock level</param>
    /// <param name="leadTimeDays">Lead time in days</param>
    /// <returns>Recommended order quantity</returns>
    int CalculateOrderQuantity(
        int averageDailyDemand, 
        int safetyStock, 
        int currentStock, 
        int leadTimeDays);
}

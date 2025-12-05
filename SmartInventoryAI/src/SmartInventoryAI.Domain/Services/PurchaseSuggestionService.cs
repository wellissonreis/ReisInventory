using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Services;

public class PurchaseSuggestionService : IPurchaseSuggestionService
{
    private const decimal HighRiskThreshold = 0.5m;
    private const int MinimumOrderMultiplier = 2;

    public PurchaseSuggestion? GenerateSuggestion(Product product, IEnumerable<Forecast> forecasts)
    {
        var forecastList = forecasts.ToList();
        
        if (forecastList.Count == 0)
            return null;

        var hasHighRisk = forecastList.Any(f => f.StockOutRisk >= HighRiskThreshold);
        
        if (!hasHighRisk && product.CurrentStock > product.SafetyStock)
            return null;

        var averageDailyDemand = (int)Math.Ceiling(forecastList.Average(f => f.PredictedDemand));
        
        if (averageDailyDemand == 0)
            return null;

        var orderQuantity = CalculateOrderQuantity(
            averageDailyDemand,
            product.SafetyStock,
            product.CurrentStock,
            product.LeadTimeDays);

        if (orderQuantity <= 0)
            return null;

        var maxRisk = forecastList.Max(f => f.StockOutRisk);
        var justification = BuildJustification(product, averageDailyDemand, maxRisk, orderQuantity);

        return PurchaseSuggestion.Create(product.Id, orderQuantity, justification);
    }

    public int CalculateOrderQuantity(
        int averageDailyDemand, 
        int safetyStock, 
        int currentStock, 
        int leadTimeDays)
    {
        var reorderPoint = (averageDailyDemand * leadTimeDays) + safetyStock;
        
        if (currentStock >= reorderPoint)
            return 0;

        var leadTimeDemand = averageDailyDemand * leadTimeDays;
        var targetStock = safetyStock + (leadTimeDemand * MinimumOrderMultiplier);
        var orderQuantity = targetStock - currentStock;

        return Math.Max(0, orderQuantity);
    }

    private static string BuildJustification(
        Product product, 
        int averageDailyDemand, 
        decimal maxRisk, 
        int orderQuantity)
    {
        var riskLevel = maxRisk switch
        {
            >= 0.7m => "ALTO",
            >= 0.4m => "MÉDIO",
            _ => "BAIXO"
        };

        return $"Sugestão de compra baseada em análise de estoque. " +
               $"Estoque atual: {product.CurrentStock} unidades. " +
               $"Estoque de segurança: {product.SafetyStock} unidades. " +
               $"Demanda média diária: {averageDailyDemand} unidades. " +
               $"Lead time: {product.LeadTimeDays} dias. " +
               $"Risco de ruptura: {riskLevel} ({maxRisk:P0}). " +
               $"Quantidade sugerida: {orderQuantity} unidades.";
    }
}

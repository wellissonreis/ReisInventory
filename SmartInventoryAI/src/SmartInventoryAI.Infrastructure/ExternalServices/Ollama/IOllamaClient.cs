using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Infrastructure.ExternalServices.Ollama;

public interface IOllamaClient
{
    /// <summary>
    /// Gets inventory advice from the AI model based on product and forecast data.
    /// </summary>
    /// <param name="product">The product to analyze</param>
    /// <param name="forecasts">Recent forecasts for the product</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated advice as a string</returns>
    Task<string> GetInventoryAdviceAsync(
        Product product, 
        IEnumerable<Forecast> forecasts, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the Ollama service is available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

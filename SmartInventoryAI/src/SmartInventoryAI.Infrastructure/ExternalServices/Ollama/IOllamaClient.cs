using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Infrastructure.ExternalServices.Ollama;

public interface IOllamaClient
{
    Task<string> GetInventoryAdviceAsync(
        Product product, 
        IEnumerable<Forecast> forecasts, 
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

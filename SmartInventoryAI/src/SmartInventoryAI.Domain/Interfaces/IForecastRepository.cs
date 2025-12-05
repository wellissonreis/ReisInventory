using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Interfaces;

public interface IForecastRepository
{
    Task<Forecast?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Forecast>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Forecast>> GetLatestByProductIdAsync(
        Guid productId, 
        int count = 10, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Forecast>> GetHighRiskForecastsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Forecast forecast, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Forecast> forecasts, CancellationToken cancellationToken = default);
    Task DeleteOldForecastsAsync(DateTime before, CancellationToken cancellationToken = default);
}

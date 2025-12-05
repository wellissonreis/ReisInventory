using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Interfaces;

public interface IStockHistoryRepository
{
    Task<StockHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockHistory>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockHistory>> GetByProductIdAndDateRangeAsync(
        Guid productId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<StockHistory>> GetRecentByProductIdAsync(
        Guid productId, 
        int days, 
        CancellationToken cancellationToken = default);
    Task AddAsync(StockHistory stockHistory, CancellationToken cancellationToken = default);
}

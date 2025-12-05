using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Domain.Interfaces;

public interface IPurchaseSuggestionRepository
{
    Task<PurchaseSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseSuggestion>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<PurchaseSuggestion?> GetLatestByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseSuggestion>> GetAllPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseSuggestion suggestion, CancellationToken cancellationToken = default);
}

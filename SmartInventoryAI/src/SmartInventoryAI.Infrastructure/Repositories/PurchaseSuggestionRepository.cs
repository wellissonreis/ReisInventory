using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Infrastructure.Data;

namespace SmartInventoryAI.Infrastructure.Repositories;

public class PurchaseSuggestionRepository : IPurchaseSuggestionRepository
{
    private readonly SmartInventoryDbContext _context;

    public PurchaseSuggestionRepository(SmartInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseSuggestions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PurchaseSuggestion>> GetByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseSuggestions
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseSuggestion?> GetLatestByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseSuggestions
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseSuggestion>> GetAllPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);
        return await _context.PurchaseSuggestions
            .Where(p => p.CreatedAt >= oneDayAgo)
            .Include(p => p.Product)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PurchaseSuggestion suggestion, CancellationToken cancellationToken = default)
    {
        await _context.PurchaseSuggestions.AddAsync(suggestion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

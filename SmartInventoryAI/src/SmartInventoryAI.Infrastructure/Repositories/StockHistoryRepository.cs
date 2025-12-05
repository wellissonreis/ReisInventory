using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Infrastructure.Data;

namespace SmartInventoryAI.Infrastructure.Repositories;

public class StockHistoryRepository : IStockHistoryRepository
{
    private readonly SmartInventoryDbContext _context;

    public StockHistoryRepository(SmartInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockHistories
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StockHistory>> GetByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.StockHistories
            .Where(s => s.ProductId == productId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockHistory>> GetByProductIdAndDateRangeAsync(
        Guid productId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.StockHistories
            .Where(s => s.ProductId == productId && 
                       s.Date >= startDate && 
                       s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockHistory>> GetRecentByProductIdAsync(
        Guid productId, 
        int days, 
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _context.StockHistories
            .Where(s => s.ProductId == productId && s.Date >= startDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StockHistory stockHistory, CancellationToken cancellationToken = default)
    {
        await _context.StockHistories.AddAsync(stockHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

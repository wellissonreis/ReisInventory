using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Infrastructure.Data;

namespace SmartInventoryAI.Infrastructure.Repositories;

public class ForecastRepository : IForecastRepository
{
    private readonly SmartInventoryDbContext _context;

    public ForecastRepository(SmartInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Forecast?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Forecasts
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Forecast>> GetByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Forecasts
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.TargetDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Forecast>> GetLatestByProductIdAsync(
        Guid productId, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Forecasts
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.CreatedAt)
            .ThenBy(f => f.TargetDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Forecast>> GetHighRiskForecastsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Forecasts
            .Where(f => f.StockOutRisk >= 0.7m)
            .Include(f => f.Product)
            .OrderByDescending(f => f.StockOutRisk)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Forecast forecast, CancellationToken cancellationToken = default)
    {
        await _context.Forecasts.AddAsync(forecast, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Forecast> forecasts, CancellationToken cancellationToken = default)
    {
        await _context.Forecasts.AddRangeAsync(forecasts, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOldForecastsAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var oldForecasts = await _context.Forecasts
            .Where(f => f.CreatedAt < before)
            .ToListAsync(cancellationToken);

        _context.Forecasts.RemoveRange(oldForecasts);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

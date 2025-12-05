using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Domain.Services;
using SmartInventoryAI.Infrastructure.Observability;
using SmartInventoryAI.Worker.Configuration;

namespace SmartInventoryAI.Worker;

public class ForecastWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ForecastWorkerOptions _options;
    private readonly ILogger<ForecastWorker> _logger;

    public ForecastWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ForecastWorkerOptions> options,
        ILogger<ForecastWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Forecast Worker starting. Interval: {IntervalMinutes} minutes, Forecast days: {ForecastDays}",
            _options.IntervalMinutes,
            _options.ForecastDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessForecastsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forecast processing");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.IntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Forecast Worker stopping");
    }

    private async Task ProcessForecastsAsync(CancellationToken cancellationToken)
    {
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("ProcessForecasts");
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting forecast processing cycle");

        using var scope = _scopeFactory.CreateScope();
        
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var stockHistoryRepository = scope.ServiceProvider.GetRequiredService<IStockHistoryRepository>();
        var forecastRepository = scope.ServiceProvider.GetRequiredService<IForecastRepository>();
        var purchaseSuggestionRepository = scope.ServiceProvider.GetRequiredService<IPurchaseSuggestionRepository>();
        var forecastService = scope.ServiceProvider.GetRequiredService<IForecastService>();
        var purchaseSuggestionService = scope.ServiceProvider.GetRequiredService<IPurchaseSuggestionService>();

        var products = await productRepository.GetAllAsync(cancellationToken);
        var productList = products.ToList();

        _logger.LogInformation("Found {ProductCount} products to process", productList.Count);
        activity?.SetTag("product.count", productList.Count);

        var processedCount = 0;
        var forecastCount = 0;
        var suggestionCount = 0;

        foreach (var product in productList)
        {
            try
            {
                using var productActivity = DiagnosticsConfig.ActivitySource.StartActivity("ProcessProductForecast");
                productActivity?.SetTag("product.id", product.Id);
                productActivity?.SetTag("product.sku", product.Sku);

                _logger.LogDebug("Processing forecasts for product {ProductId} ({ProductName})", product.Id, product.Name);

                var stockHistory = await stockHistoryRepository.GetRecentByProductIdAsync(
                    product.Id, 
                    _options.HistoryDays, 
                    cancellationToken);

                var forecasts = forecastService.GenerateForecasts(
                    product, 
                    stockHistory, 
                    _options.ForecastDays);

                var forecastList = forecasts.ToList();

                if (forecastList.Any())
                {
                    await forecastRepository.AddRangeAsync(forecastList, cancellationToken);
                    forecastCount += forecastList.Count;

                    _logger.LogDebug(
                        "Generated {ForecastCount} forecasts for product {ProductId}",
                        forecastList.Count,
                        product.Id);
                }

                if (_options.GeneratePurchaseSuggestions && forecastList.Any())
                {
                    var suggestion = purchaseSuggestionService.GenerateSuggestion(product, forecastList);
                    
                    if (suggestion != null)
                    {
                        await purchaseSuggestionRepository.AddAsync(suggestion, cancellationToken);
                        suggestionCount++;

                        _logger.LogInformation(
                            "Generated purchase suggestion for product {ProductId}: {Quantity} units",
                            product.Id,
                            suggestion.SuggestedQuantity);
                    }
                }

                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing product {ProductId}", product.Id);
            }
        }

        stopwatch.Stop();

        activity?.SetTag("products.processed", processedCount);
        activity?.SetTag("forecasts.generated", forecastCount);
        activity?.SetTag("suggestions.generated", suggestionCount);
        activity?.SetTag("duration.ms", stopwatch.ElapsedMilliseconds);

        _logger.LogInformation(
            "Forecast processing completed. Products: {ProcessedCount}, Forecasts: {ForecastCount}, Suggestions: {SuggestionCount}, Duration: {Duration}ms",
            processedCount,
            forecastCount,
            suggestionCount,
            stopwatch.ElapsedMilliseconds);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            await forecastRepository.DeleteOldForecastsAsync(cutoffDate, cancellationToken);
            _logger.LogDebug("Cleaned up old forecasts before {CutoffDate}", cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up old forecasts");
        }
    }
}

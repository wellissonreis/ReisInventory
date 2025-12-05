using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Domain.Services;
using SmartInventoryAI.Infrastructure.Caching;
using SmartInventoryAI.Infrastructure.Data;
using SmartInventoryAI.Infrastructure.ExternalServices.Ollama;
using SmartInventoryAI.Infrastructure.Observability;
using SmartInventoryAI.Infrastructure.Repositories;

namespace SmartInventoryAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<SmartInventoryDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(SmartInventoryDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
        });

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockHistoryRepository, StockHistoryRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<IPurchaseSuggestionRepository, PurchaseSuggestionRepository>();

        // Domain Services
        services.AddScoped<IForecastService, ForecastService>();
        services.AddScoped<IPurchaseSuggestionService, PurchaseSuggestionService>();

        // Redis Cache
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() 
            ?? new RedisOptions();
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName = redisOptions.InstanceName;
        });

        // Ollama Client
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        
        var ollamaOptions = configuration.GetSection(OllamaOptions.SectionName).Get<OllamaOptions>() 
            ?? new OllamaOptions();

        services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
        {
            client.BaseAddress = new Uri(ollamaOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds);
        });

        // Observability
        services.AddObservability(configuration);

        return services;
    }
}

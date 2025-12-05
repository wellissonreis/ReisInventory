using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartInventoryAI.Infrastructure;
using SmartInventoryAI.Worker;
using SmartInventoryAI.Worker.Configuration;

namespace SmartInventoryAI.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure worker settings
        builder.Services.Configure<ForecastWorkerOptions>(
            builder.Configuration.GetSection(ForecastWorkerOptions.SectionName));

        // Add Infrastructure services (DbContext, Repositories, Ollama, Redis, OpenTelemetry)
        builder.Services.AddInfrastructure(builder.Configuration);

        // Add hosted services
        builder.Services.AddHostedService<ForecastWorker>();

        var host = builder.Build();
        host.Run();
    }
}

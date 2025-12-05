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

        builder.Services.Configure<ForecastWorkerOptions>(
            builder.Configuration.GetSection(ForecastWorkerOptions.SectionName));

        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddHostedService<ForecastWorker>();

        var host = builder.Build();
        host.Run();
    }
}

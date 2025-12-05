using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SmartInventoryAI.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration,
        string serviceName = DiagnosticsConfig.ServiceName)
    {
        var jaegerOptions = configuration.GetSection(JaegerOptions.SectionName).Get<JaegerOptions>() 
            ?? new JaegerOptions();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: DiagnosticsConfig.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(DiagnosticsConfig.ActivitySource.Name)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(jaegerOptions.Endpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(jaegerOptions.Endpoint);
                    });
            });

        return services;
    }
}

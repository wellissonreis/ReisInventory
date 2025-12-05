using System.Diagnostics;

namespace SmartInventoryAI.Infrastructure.Observability;

public static class DiagnosticsConfig
{
    public const string ServiceName = "SmartInventoryAI";
    public const string ServiceVersion = "1.0.0";
    
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);
}

namespace SmartInventoryAI.Infrastructure.Observability;

public class JaegerOptions
{
    public const string SectionName = "Jaeger";
    
    public string Endpoint { get; set; } = "http://localhost:4317";
}

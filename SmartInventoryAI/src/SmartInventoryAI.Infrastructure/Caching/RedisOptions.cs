namespace SmartInventoryAI.Infrastructure.Caching;

public class RedisOptions
{
    public const string SectionName = "Redis";
    
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "SmartInventoryAI_";
}

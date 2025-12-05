namespace SmartInventoryAI.Infrastructure.ExternalServices.Ollama;

public class OllamaOptions
{
    public const string SectionName = "Ollama";
    
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "mistral:7b";
    public int TimeoutSeconds { get; set; } = 120;
}

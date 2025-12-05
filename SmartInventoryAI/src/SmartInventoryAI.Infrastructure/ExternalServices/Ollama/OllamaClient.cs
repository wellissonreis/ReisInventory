using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartInventoryAI.Domain.Entities;

namespace SmartInventoryAI.Infrastructure.ExternalServices.Ollama;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(
        HttpClient httpClient, 
        IOptions<OllamaOptions> options,
        ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetInventoryAdviceAsync(
        Product product, 
        IEnumerable<Forecast> forecasts, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = BuildInventoryPrompt(product, forecasts);
            
            var request = new OllamaGenerateRequest
            {
                Model = _options.ModelName,
                Prompt = prompt,
                Stream = false,
                Options = new OllamaRequestOptions
                {
                    Temperature = 0.7,
                    NumPredict = 500
                }
            };

            _logger.LogInformation(
                "Requesting AI advice for product {ProductId} ({ProductName})", 
                product.Id, 
                product.Name);

            var response = await _httpClient.PostAsJsonAsync(
                "/api/generate", 
                request, 
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("Empty response from Ollama for product {ProductId}", product.Id);
                return "Não foi possível obter recomendação da IA no momento.";
            }

            _logger.LogInformation(
                "Received AI advice for product {ProductId}, response length: {Length}", 
                product.Id, 
                result.Response.Length);

            return result.Response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Ollama API for product {ProductId}", product.Id);
            return "Serviço de IA temporariamente indisponível. Por favor, tente novamente mais tarde.";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout calling Ollama API for product {ProductId}", product.Id);
            return "Tempo limite excedido ao consultar a IA. Por favor, tente novamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama API for product {ProductId}", product.Id);
            return "Erro inesperado ao consultar a IA. Por favor, tente novamente.";
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildInventoryPrompt(Product product, IEnumerable<Forecast> forecasts)
    {
        var forecastList = forecasts.ToList();
        var sb = new StringBuilder();

        sb.AppendLine("Você é um assistente especializado em gestão de estoque e cadeia de suprimentos.");
        sb.AppendLine("Analise os dados do produto e forneça recomendações de compra.");
        sb.AppendLine();
        sb.AppendLine("=== DADOS DO PRODUTO ===");
        sb.AppendLine($"Nome: {product.Name}");
        sb.AppendLine($"SKU: {product.Sku}");
        sb.AppendLine($"Categoria: {product.Category}");
        sb.AppendLine($"Estoque Atual: {product.CurrentStock} unidades");
        sb.AppendLine($"Estoque de Segurança: {product.SafetyStock} unidades");
        sb.AppendLine($"Lead Time: {product.LeadTimeDays} dias");
        sb.AppendLine();

        if (forecastList.Any())
        {
            sb.AppendLine("=== PREVISÕES DE DEMANDA ===");
            foreach (var forecast in forecastList.Take(7))
            {
                var riskLevel = forecast.StockOutRisk switch
                {
                    >= 0.7m => "ALTO",
                    >= 0.4m => "MÉDIO",
                    _ => "BAIXO"
                };
                sb.AppendLine($"- {forecast.TargetDate:dd/MM/yyyy}: Demanda prevista: {forecast.PredictedDemand} unidades, Risco de ruptura: {riskLevel} ({forecast.StockOutRisk:P0})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== SOLICITAÇÃO ===");
        sb.AppendLine("Com base nos dados acima, forneça:");
        sb.AppendLine("1. Uma análise resumida da situação do estoque");
        sb.AppendLine("2. Quantidade recomendada para compra (se necessário)");
        sb.AppendLine("3. Justificativa para a recomendação");
        sb.AppendLine("4. Urgência da ação (Alta/Média/Baixa)");
        sb.AppendLine();
        sb.AppendLine("Responda de forma objetiva e profissional em português.");

        return sb.ToString();
    }
}

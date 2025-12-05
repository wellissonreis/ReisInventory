using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartInventoryAI.Api.DTOs.AI;
using SmartInventoryAI.Domain.Interfaces;
using SmartInventoryAI.Domain.Services;
using SmartInventoryAI.Infrastructure.ExternalServices.Ollama;

namespace SmartInventoryAI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public partial class AIController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IForecastRepository _forecastRepository;
    private readonly IPurchaseSuggestionRepository _purchaseSuggestionRepository;
    private readonly IPurchaseSuggestionService _purchaseSuggestionService;
    private readonly IOllamaClient _ollamaClient;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IProductRepository productRepository,
        IForecastRepository forecastRepository,
        IPurchaseSuggestionRepository purchaseSuggestionRepository,
        IPurchaseSuggestionService purchaseSuggestionService,
        IOllamaClient ollamaClient,
        ILogger<AIController> logger)
    {
        _productRepository = productRepository;
        _forecastRepository = forecastRepository;
        _purchaseSuggestionRepository = purchaseSuggestionRepository;
        _purchaseSuggestionService = purchaseSuggestionService;
        _ollamaClient = ollamaClient;
        _logger = logger;
    }

    [HttpGet("suggestions/{productId:guid}")]
    [ProducesResponseType(typeof(AISuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AISuggestionResponse>> GetSuggestion(
        Guid productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting AI suggestion for product {ProductId}", productId);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {productId} não encontrado." });
        }

        var forecasts = await _forecastRepository.GetLatestByProductIdAsync(productId, 7, cancellationToken);
        var forecastList = forecasts.ToList();

        var aiAdvice = await _ollamaClient.GetInventoryAdviceAsync(product, forecastList, cancellationToken);

        int? suggestedQuantity = ExtractSuggestedQuantity(aiAdvice);

        if (!suggestedQuantity.HasValue && forecastList.Any())
        {
            var suggestion = _purchaseSuggestionService.GenerateSuggestion(product, forecastList);
            suggestedQuantity = suggestion?.SuggestedQuantity;
        }

        var response = new AISuggestionResponse(
            productId,
            product.Name,
            suggestedQuantity,
            aiAdvice,
            DateTime.UtcNow);

        return Ok(response);
    }

    [HttpGet("suggestions/{productId:guid}/latest")]
    [ProducesResponseType(typeof(AISuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AISuggestionResponse>> GetLatestSuggestion(
        Guid productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting latest saved suggestion for product {ProductId}", productId);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {productId} não encontrado." });
        }

        var suggestion = await _purchaseSuggestionRepository.GetLatestByProductIdAsync(productId, cancellationToken);
        if (suggestion == null)
        {
            return NotFound(new { message = $"Nenhuma sugestão encontrada para o produto {productId}." });
        }

        var response = new AISuggestionResponse(
            productId,
            product.Name,
            suggestion.SuggestedQuantity,
            suggestion.Justification,
            suggestion.CreatedAt);

        return Ok(response);
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckHealth(CancellationToken cancellationToken)
    {
        var isAvailable = await _ollamaClient.IsAvailableAsync(cancellationToken);
        
        if (isAvailable)
        {
            return Ok(new { status = "healthy", message = "Serviço de IA está disponível." });
        }

        return StatusCode(503, new { status = "unhealthy", message = "Serviço de IA não está disponível." });
    }

    private static int? ExtractSuggestedQuantity(string aiResponse)
    {
        var patterns = new[]
        {
            @"(?:quantidade|quantia|comprar|pedir|solicitar|recomendar?o?)[\s:]*(\d+)",
            @"(\d+)\s*(?:unidades|peças|itens)",
            @"(?:sugir?o|recomendo)[\s:]*(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(aiResponse, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var quantity) && quantity > 0)
            {
                return quantity;
            }
        }

        return null;
    }
}

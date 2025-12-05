using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartInventoryAI.Api.DTOs.Forecasts;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;

namespace SmartInventoryAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly IForecastRepository _forecastRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(
        IForecastRepository forecastRepository,
        IProductRepository productRepository,
        ILogger<ForecastController> logger)
    {
        _forecastRepository = forecastRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém as previsões de um produto.
    /// </summary>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ForecastResponse>>> GetByProductId(
        Guid productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching forecasts for product {ProductId}", productId);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {productId} não encontrado." });
        }

        var forecasts = await _forecastRepository.GetLatestByProductIdAsync(productId, 14, cancellationToken);
        var response = forecasts.Select(MapToResponse);

        return Ok(response);
    }

    /// <summary>
    /// Obtém todas as previsões de alto risco.
    /// </summary>
    [HttpGet("high-risk")]
    [ProducesResponseType(typeof(IEnumerable<ForecastResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ForecastResponse>>> GetHighRisk(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching high risk forecasts");

        var forecasts = await _forecastRepository.GetHighRiskForecastsAsync(cancellationToken);
        var response = forecasts.Select(MapToResponse);

        return Ok(response);
    }

    private static ForecastResponse MapToResponse(Forecast forecast)
    {
        var riskLevel = forecast.StockOutRisk switch
        {
            >= 0.7m => "Alto",
            >= 0.4m => "Médio",
            _ => "Baixo"
        };

        return new ForecastResponse(
            forecast.Id,
            forecast.ProductId,
            forecast.TargetDate,
            forecast.PredictedDemand,
            forecast.StockOutRisk,
            riskLevel,
            forecast.CreatedAt);
    }
}

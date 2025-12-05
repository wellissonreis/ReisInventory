using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartInventoryAI.Api.DTOs.Stock;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;

namespace SmartInventoryAI.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockHistoryRepository _stockHistoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<StockController> _logger;

    public StockController(
        IStockHistoryRepository stockHistoryRepository,
        IProductRepository productRepository,
        ILogger<StockController> logger)
    {
        _stockHistoryRepository = stockHistoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o histórico de movimentação de um produto.
    /// </summary>
    [HttpGet("history/{productId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<StockHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<StockHistoryResponse>>> GetHistory(
        Guid productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock history for product {ProductId}", productId);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {productId} não encontrado." });
        }

        var history = await _stockHistoryRepository.GetByProductIdAsync(productId, cancellationToken);
        var response = history.Select(MapToResponse);

        return Ok(response);
    }

    /// <summary>
    /// Obtém o histórico de movimentação de um produto nos últimos N dias.
    /// </summary>
    [HttpGet("history/{productId:guid}/recent")]
    [ProducesResponseType(typeof(IEnumerable<StockHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<StockHistoryResponse>>> GetRecentHistory(
        Guid productId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching recent stock history for product {ProductId}, last {Days} days", productId, days);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {productId} não encontrado." });
        }

        var history = await _stockHistoryRepository.GetRecentByProductIdAsync(productId, days, cancellationToken);
        var response = history.Select(MapToResponse);

        return Ok(response);
    }

    /// <summary>
    /// Registra uma movimentação de estoque.
    /// </summary>
    [HttpPost("history")]
    [ProducesResponseType(typeof(StockHistoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockHistoryResponse>> CreateHistory(
        [FromBody] CreateStockHistoryRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating stock history for product {ProductId}, quantity change: {QuantityChange}",
            request.ProductId,
            request.QuantityChange);

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Produto com ID {request.ProductId} não encontrado." });
        }

        try
        {
            var stockHistory = StockHistory.Create(
                request.ProductId,
                request.QuantityChange,
                request.Reason,
                request.Date);

            await _stockHistoryRepository.AddAsync(stockHistory, cancellationToken);

            // Update product current stock
            var newStock = product.CurrentStock + request.QuantityChange;
            product.UpdateStock(newStock);
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation(
                "Stock history created with ID {HistoryId}, new stock level: {NewStock}",
                stockHistory.Id,
                newStock);

            return CreatedAtAction(
                nameof(GetHistory),
                new { productId = request.ProductId },
                MapToResponse(stockHistory));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid stock history data");
            return BadRequest(new { message = ex.Message });
        }
    }

    private static StockHistoryResponse MapToResponse(StockHistory history)
    {
        var movementType = history.QuantityChange > 0 ? "Entrada" : "Saída";
        
        return new StockHistoryResponse(
            history.Id,
            history.ProductId,
            history.Date,
            history.QuantityChange,
            history.Reason,
            movementType);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartInventoryAI.Api.DTOs.Products;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Interfaces;

namespace SmartInventoryAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os produtos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all products");
        
        var products = await _productRepository.GetAllAsync(cancellationToken);
        var response = products.Select(MapToResponse);
        
        return Ok(response);
    }

    /// <summary>
    /// Obtém um produto pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product {ProductId}", id);
        
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return NotFound(new { message = $"Produto com ID {id} não encontrado." });
        }
        
        return Ok(MapToResponse(product));
    }

    /// <summary>
    /// Obtém um produto pelo SKU.
    /// </summary>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetBySku(
        string sku, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product by SKU {Sku}", sku);
        
        var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product with SKU {Sku} not found", sku);
            return NotFound(new { message = $"Produto com SKU {sku} não encontrado." });
        }
        
        return Ok(MapToResponse(product));
    }

    /// <summary>
    /// Lista produtos com estoque baixo.
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetLowStock(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching low stock products");
        
        var products = await _productRepository.GetLowStockProductsAsync(cancellationToken);
        var response = products.Select(MapToResponse);
        
        return Ok(response);
    }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with SKU {Sku}", request.Sku);

        // Check if SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null)
        {
            _logger.LogWarning("Product with SKU {Sku} already exists", request.Sku);
            return Conflict(new { message = $"Produto com SKU {request.Sku} já existe." });
        }

        try
        {
            var product = Product.Create(
                request.Sku,
                request.Name,
                request.Category,
                request.CurrentStock,
                request.SafetyStock,
                request.LeadTimeDays);

            await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Product created with ID {ProductId}", product.Id);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                MapToResponse(product));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid product data");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for update", id);
            return NotFound(new { message = $"Produto com ID {id} não encontrado." });
        }

        try
        {
            product.Update(
                request.Name,
                request.Category,
                request.SafetyStock,
                request.LeadTimeDays);

            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product {ProductId} updated", id);

            return Ok(MapToResponse(product));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid product update data");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove um produto.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for deletion", id);
            return NotFound(new { message = $"Produto com ID {id} não encontrado." });
        }

        await _productRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Product {ProductId} deleted", id);

        return NoContent();
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.Category,
            product.CurrentStock,
            product.SafetyStock,
            product.LeadTimeDays,
            product.IsLowStock(),
            product.CreatedAt,
            product.UpdatedAt);
    }
}

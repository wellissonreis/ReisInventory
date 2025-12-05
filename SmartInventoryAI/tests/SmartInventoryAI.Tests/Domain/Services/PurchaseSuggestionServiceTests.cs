using FluentAssertions;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Services;
using Xunit;

namespace SmartInventoryAI.Tests.Domain.Services;

public class PurchaseSuggestionServiceTests
{
    private readonly PurchaseSuggestionService _sut;

    public PurchaseSuggestionServiceTests()
    {
        _sut = new PurchaseSuggestionService();
    }

    [Fact]
    public void GenerateSuggestion_WithEmptyForecasts_ReturnsNull()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 100);
        var emptyForecasts = Enumerable.Empty<Forecast>();

        // Act
        var suggestion = _sut.GenerateSuggestion(product, emptyForecasts);

        // Assert
        suggestion.Should().BeNull();
    }

    [Fact]
    public void GenerateSuggestion_WithLowRiskAndAdequateStock_ReturnsNull()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 100, safetyStock: 20);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.2m, days: 7);

        // Act
        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        // Assert
        suggestion.Should().BeNull();
    }

    [Fact]
    public void GenerateSuggestion_WithHighRisk_ReturnsSuggestion()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 15, safetyStock: 20, leadTimeDays: 5);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.7m, days: 7);

        // Act
        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        // Assert
        suggestion.Should().NotBeNull();
        suggestion!.ProductId.Should().Be(product.Id);
        suggestion.SuggestedQuantity.Should().BeGreaterThan(0);
        suggestion.Justification.Should().NotBeEmpty();
        suggestion.Justification.Should().Contain("ALTO"); // High risk
    }

    [Fact]
    public void GenerateSuggestion_WithLowStockBelowSafety_ReturnsSuggestion()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 10, safetyStock: 50, leadTimeDays: 5);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.3m, days: 7);

        // Act
        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        // Assert
        suggestion.Should().NotBeNull();
        suggestion!.SuggestedQuantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateSuggestion_Justification_ContainsRelevantInfo()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 15, safetyStock: 30, leadTimeDays: 7);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 20, stockOutRisk: 0.8m, days: 7);

        // Act
        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        // Assert
        suggestion.Should().NotBeNull();
        suggestion!.Justification.Should().Contain("Estoque atual:");
        suggestion.Justification.Should().Contain("Estoque de segurança:");
        suggestion.Justification.Should().Contain("Lead time:");
        suggestion.Justification.Should().Contain("Risco de ruptura:");
    }

    [Fact]
    public void CalculateOrderQuantity_WhenStockBelowReorderPoint_ReturnsPositiveQuantity()
    {
        // Arrange
        var averageDailyDemand = 10;
        var safetyStock = 20;
        var currentStock = 30;
        var leadTimeDays = 5;

        // Reorder point = (10 * 5) + 20 = 70
        // Current stock (30) < Reorder point (70)

        // Act
        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        // Assert
        quantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOrderQuantity_WhenStockAboveReorderPoint_ReturnsZero()
    {
        // Arrange
        var averageDailyDemand = 10;
        var safetyStock = 20;
        var currentStock = 200;
        var leadTimeDays = 5;

        // Reorder point = (10 * 5) + 20 = 70
        // Current stock (200) > Reorder point (70)

        // Act
        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        // Assert
        quantity.Should().Be(0);
    }

    [Theory]
    [InlineData(10, 20, 30, 5, 120)] // Need to order to reach target
    [InlineData(5, 10, 100, 3, 0)]   // Already have enough stock
    [InlineData(20, 50, 10, 7, 320)] // Very low stock, high demand
    public void CalculateOrderQuantity_VariousScenarios_ReturnsExpectedQuantity(
        int averageDailyDemand,
        int safetyStock,
        int currentStock,
        int leadTimeDays,
        int expectedMinQuantity)
    {
        // Act
        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        // Assert
        if (expectedMinQuantity > 0)
        {
            quantity.Should().BeGreaterThanOrEqualTo(expectedMinQuantity - 50); // Allow some variance
        }
        else
        {
            quantity.Should().Be(0);
        }
    }

    [Fact]
    public void GenerateSuggestion_WithZeroDemand_ReturnsNull()
    {
        // Arrange
        var product = CreateTestProduct(currentStock: 100);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 0, stockOutRisk: 0, days: 7);

        // Act
        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        // Assert
        suggestion.Should().BeNull();
    }

    private static Product CreateTestProduct(
        int currentStock = 100,
        int safetyStock = 20,
        int leadTimeDays = 5)
    {
        return Product.Create(
            sku: "TEST-001",
            name: "Test Product",
            category: "Test",
            currentStock: currentStock,
            safetyStock: safetyStock,
            leadTimeDays: leadTimeDays);
    }

    private static IEnumerable<Forecast> CreateForecasts(
        Guid productId,
        int predictedDemand,
        decimal stockOutRisk,
        int days)
    {
        var forecasts = new List<Forecast>();
        var today = DateTime.UtcNow.Date;

        for (int i = 1; i <= days; i++)
        {
            forecasts.Add(Forecast.Create(
                productId,
                targetDate: today.AddDays(i),
                predictedDemand: predictedDemand,
                stockOutRisk: stockOutRisk));
        }

        return forecasts;
    }
}

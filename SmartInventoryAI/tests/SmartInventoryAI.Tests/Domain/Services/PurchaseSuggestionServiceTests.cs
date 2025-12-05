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
        var product = CreateTestProduct(currentStock: 100);
        var emptyForecasts = Enumerable.Empty<Forecast>();

        var suggestion = _sut.GenerateSuggestion(product, emptyForecasts);

        suggestion.Should().BeNull();
    }

    [Fact]
    public void GenerateSuggestion_WithLowRiskAndAdequateStock_ReturnsNull()
    {
        var product = CreateTestProduct(currentStock: 100, safetyStock: 20);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.2m, days: 7);

        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        suggestion.Should().BeNull();
    }

    [Fact]
    public void GenerateSuggestion_WithHighRisk_ReturnsSuggestion()
    {
        var product = CreateTestProduct(currentStock: 15, safetyStock: 20, leadTimeDays: 5);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.7m, days: 7);

        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        suggestion.Should().NotBeNull();
        suggestion!.ProductId.Should().Be(product.Id);
        suggestion.SuggestedQuantity.Should().BeGreaterThan(0);
        suggestion.Justification.Should().NotBeEmpty();
        suggestion.Justification.Should().Contain("ALTO");
    }

    [Fact]
    public void GenerateSuggestion_WithLowStockBelowSafety_ReturnsSuggestion()
    {
        var product = CreateTestProduct(currentStock: 10, safetyStock: 50, leadTimeDays: 5);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 10, stockOutRisk: 0.3m, days: 7);

        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        suggestion.Should().NotBeNull();
        suggestion!.SuggestedQuantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateSuggestion_Justification_ContainsRelevantInfo()
    {
        var product = CreateTestProduct(currentStock: 15, safetyStock: 30, leadTimeDays: 7);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 20, stockOutRisk: 0.8m, days: 7);

        var suggestion = _sut.GenerateSuggestion(product, forecasts);

        suggestion.Should().NotBeNull();
        suggestion!.Justification.Should().Contain("Estoque atual:");
        suggestion.Justification.Should().Contain("Estoque de segurança:");
        suggestion.Justification.Should().Contain("Lead time:");
        suggestion.Justification.Should().Contain("Risco de ruptura:");
    }

    [Fact]
    public void CalculateOrderQuantity_WhenStockBelowReorderPoint_ReturnsPositiveQuantity()
    {
        var averageDailyDemand = 10;
        var safetyStock = 20;
        var currentStock = 30;
        var leadTimeDays = 5;

        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        quantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOrderQuantity_WhenStockAboveReorderPoint_ReturnsZero()
    {
        var averageDailyDemand = 10;
        var safetyStock = 20;
        var currentStock = 200;
        var leadTimeDays = 5;

        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        quantity.Should().Be(0);
    }

    [Theory]
    [InlineData(10, 20, 30, 5, 120)]
    [InlineData(5, 10, 100, 3, 0)]
    [InlineData(20, 50, 10, 7, 320)]
    public void CalculateOrderQuantity_VariousScenarios_ReturnsExpectedQuantity(
        int averageDailyDemand,
        int safetyStock,
        int currentStock,
        int leadTimeDays,
        int expectedMinQuantity)
    {
        var quantity = _sut.CalculateOrderQuantity(averageDailyDemand, safetyStock, currentStock, leadTimeDays);

        if (expectedMinQuantity > 0)
        {
            quantity.Should().BeGreaterThanOrEqualTo(expectedMinQuantity - 50);
        }
        else
        {
            quantity.Should().Be(0);
        }
    }

    [Fact]
    public void GenerateSuggestion_WithZeroDemand_ReturnsNull()
    {
        var product = CreateTestProduct(currentStock: 100);
        var forecasts = CreateForecasts(product.Id, predictedDemand: 0, stockOutRisk: 0, days: 7);

        var suggestion = _sut.GenerateSuggestion(product, forecasts);

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

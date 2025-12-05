using FluentAssertions;
using SmartInventoryAI.Domain.Entities;
using SmartInventoryAI.Domain.Services;
using Xunit;

namespace SmartInventoryAI.Tests.Domain.Services;

public class ForecastServiceTests
{
    private readonly ForecastService _sut;

    public ForecastServiceTests()
    {
        _sut = new ForecastService();
    }

    [Fact]
    public void GenerateForecasts_WithNoHistory_ReturnsZeroDemandForecasts()
    {
        var product = CreateTestProduct(currentStock: 100, safetyStock: 20);
        var emptyHistory = Enumerable.Empty<StockHistory>();

        var forecasts = _sut.GenerateForecasts(product, emptyHistory, forecastDays: 7);

        var forecastList = forecasts.ToList();
        forecastList.Should().HaveCount(7);
        forecastList.Should().AllSatisfy(f =>
        {
            f.PredictedDemand.Should().Be(0);
            f.StockOutRisk.Should().Be(0);
        });
    }

    [Fact]
    public void GenerateForecasts_WithSalesHistory_CalculatesAverageDemand()
    {
        var product = CreateTestProduct(currentStock: 100, safetyStock: 20);
        var history = CreateSalesHistory(product.Id, dailySales: 10, days: 7);

        var forecasts = _sut.GenerateForecasts(product, history, forecastDays: 7);

        var forecastList = forecasts.ToList();
        forecastList.Should().HaveCount(7);
        forecastList.Should().AllSatisfy(f =>
        {
            f.PredictedDemand.Should().Be(10);
            f.ProductId.Should().Be(product.Id);
        });
    }

    [Fact]
    public void GenerateForecasts_WithLowStock_CalculatesHighRisk()
    {
        var product = CreateTestProduct(currentStock: 15, safetyStock: 20, leadTimeDays: 5);
        var history = CreateSalesHistory(product.Id, dailySales: 10, days: 7);

        var forecasts = _sut.GenerateForecasts(product, history, forecastDays: 7);

        var forecastList = forecasts.ToList();
        forecastList.Should().HaveCount(7);
        
        var laterForecast = forecastList.Last();
        laterForecast.StockOutRisk.Should().BeGreaterThan(0.5m);
    }

    [Fact]
    public void GenerateForecasts_WithAbundantStock_CalculatesLowRisk()
    {
        var product = CreateTestProduct(currentStock: 1000, safetyStock: 50, leadTimeDays: 5);
        var history = CreateSalesHistory(product.Id, dailySales: 10, days: 7);

        var forecasts = _sut.GenerateForecasts(product, history, forecastDays: 7);

        var forecastList = forecasts.ToList();
        forecastList.Should().AllSatisfy(f =>
        {
            f.StockOutRisk.Should().BeLessThan(0.4m);
        });
    }

    [Theory]
    [InlineData(0, 10, 50, 5, 1.0)]
    [InlineData(100, 10, 50, 5, 0.0)]
    [InlineData(20, 10, 50, 5, 1.0)]
    public void CalculateStockOutRisk_VariousScenarios_ReturnsExpectedRisk(
        int currentStock,
        int safetyStock,
        int predictedDemand,
        int leadTimeDays,
        decimal expectedMinRisk)
    {
        var risk = _sut.CalculateStockOutRisk(currentStock, safetyStock, predictedDemand, leadTimeDays);

        risk.Should().BeGreaterThanOrEqualTo(expectedMinRisk - 0.1m);
        risk.Should().BeInRange(0, 1);
    }

    [Fact]
    public void CalculateStockOutRisk_AlwaysReturnsBetweenZeroAndOne()
    {
        var scenarios = new[]
        {
            (currentStock: 0, safetyStock: 10, predictedDemand: 100, leadTimeDays: 5),
            (currentStock: 1000, safetyStock: 10, predictedDemand: 1, leadTimeDays: 1),
            (currentStock: 50, safetyStock: 50, predictedDemand: 50, leadTimeDays: 10),
            (currentStock: 10, safetyStock: 100, predictedDemand: 200, leadTimeDays: 30),
        };

        foreach (var (currentStock, safetyStock, predictedDemand, leadTimeDays) in scenarios)
        {
            var risk = _sut.CalculateStockOutRisk(currentStock, safetyStock, predictedDemand, leadTimeDays);
            risk.Should().BeInRange(0, 1, 
                $"Risk should be 0-1 for scenario: stock={currentStock}, safety={safetyStock}, demand={predictedDemand}");
        }
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

    private static IEnumerable<StockHistory> CreateSalesHistory(
        Guid productId,
        int dailySales,
        int days)
    {
        var history = new List<StockHistory>();
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i < days; i++)
        {
            history.Add(StockHistory.Create(
                productId,
                quantityChange: -dailySales,
                reason: "sale",
                date: today.AddDays(-i)));
        }

        return history;
    }
}

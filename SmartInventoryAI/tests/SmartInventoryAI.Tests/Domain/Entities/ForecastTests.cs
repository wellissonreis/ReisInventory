using FluentAssertions;
using SmartInventoryAI.Domain.Entities;
using Xunit;

namespace SmartInventoryAI.Tests.Domain.Entities;

public class ForecastTests
{
    [Fact]
    public void Create_WithValidData_ReturnsForecast()
    {
        var productId = Guid.NewGuid();
        var targetDate = DateTime.UtcNow.AddDays(1);

        var forecast = Forecast.Create(
            productId: productId,
            targetDate: targetDate,
            predictedDemand: 100,
            stockOutRisk: 0.5m);

        forecast.Should().NotBeNull();
        forecast.Id.Should().NotBe(Guid.Empty);
        forecast.ProductId.Should().Be(productId);
        forecast.TargetDate.Should().Be(targetDate);
        forecast.PredictedDemand.Should().Be(100);
        forecast.StockOutRisk.Should().Be(0.5m);
    }

    [Fact]
    public void Create_WithEmptyProductId_ThrowsArgumentException()
    {
        var act = () => Forecast.Create(
            productId: Guid.Empty,
            targetDate: DateTime.UtcNow.AddDays(1),
            predictedDemand: 100,
            stockOutRisk: 0.5m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("productId");
    }

    [Fact]
    public void Create_WithNegativeDemand_ThrowsArgumentException()
    {
        var act = () => Forecast.Create(
            productId: Guid.NewGuid(),
            targetDate: DateTime.UtcNow.AddDays(1),
            predictedDemand: -10,
            stockOutRisk: 0.5m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("predictedDemand");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Create_WithInvalidRisk_ThrowsArgumentException(decimal risk)
    {
        var act = () => Forecast.Create(
            productId: Guid.NewGuid(),
            targetDate: DateTime.UtcNow.AddDays(1),
            predictedDemand: 100,
            stockOutRisk: risk);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("stockOutRisk");
    }

    [Theory]
    [InlineData(0.7, true)]
    [InlineData(0.8, true)]
    [InlineData(1.0, true)]
    [InlineData(0.69, false)]
    [InlineData(0.5, false)]
    public void IsHighRisk_ReturnsCorrectValue(decimal risk, bool expected)
    {
        var forecast = Forecast.Create(
            productId: Guid.NewGuid(),
            targetDate: DateTime.UtcNow.AddDays(1),
            predictedDemand: 100,
            stockOutRisk: risk);

        forecast.IsHighRisk().Should().Be(expected);
    }

    [Theory]
    [InlineData(0.4, true)]
    [InlineData(0.5, true)]
    [InlineData(0.69, true)]
    [InlineData(0.39, false)]
    [InlineData(0.7, false)]
    public void IsMediumRisk_ReturnsCorrectValue(decimal risk, bool expected)
    {
        var forecast = Forecast.Create(
            productId: Guid.NewGuid(),
            targetDate: DateTime.UtcNow.AddDays(1),
            predictedDemand: 100,
            stockOutRisk: risk);

        forecast.IsMediumRisk().Should().Be(expected);
    }
}

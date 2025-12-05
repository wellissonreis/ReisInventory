using FluentAssertions;
using SmartInventoryAI.Domain.Entities;
using Xunit;

namespace SmartInventoryAI.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsProduct()
    {
        var product = Product.Create(
            sku: "SKU-001",
            name: "Test Product",
            category: "Electronics",
            currentStock: 100,
            safetyStock: 20,
            leadTimeDays: 5);

        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.Sku.Should().Be("SKU-001");
        product.Name.Should().Be("Test Product");
        product.Category.Should().Be("Electronics");
        product.CurrentStock.Should().Be(100);
        product.SafetyStock.Should().Be(20);
        product.LeadTimeDays.Should().Be(5);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptySku_ThrowsArgumentException()
    {
        var act = () => Product.Create(
            sku: "",
            name: "Test Product",
            category: "Electronics",
            currentStock: 100,
            safetyStock: 20,
            leadTimeDays: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("sku");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => Product.Create(
            sku: "SKU-001",
            name: "",
            category: "Electronics",
            currentStock: 100,
            safetyStock: 20,
            leadTimeDays: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Create_WithNegativeSafetyStock_ThrowsArgumentException()
    {
        var act = () => Product.Create(
            sku: "SKU-001",
            name: "Test Product",
            category: "Electronics",
            currentStock: 100,
            safetyStock: -10,
            leadTimeDays: 5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("safetyStock");
    }

    [Fact]
    public void IsLowStock_WhenStockBelowSafety_ReturnsTrue()
    {
        var product = Product.Create(
            sku: "SKU-001",
            name: "Test Product",
            category: "Electronics",
            currentStock: 15,
            safetyStock: 20,
            leadTimeDays: 5);

        product.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenStockAboveSafety_ReturnsFalse()
    {
        var product = Product.Create(
            sku: "SKU-001",
            name: "Test Product",
            category: "Electronics",
            currentStock: 100,
            safetyStock: 20,
            leadTimeDays: 5);

        product.IsLowStock().Should().BeFalse();
    }

    [Fact]
    public void UpdateStock_UpdatesCurrentStockAndTimestamp()
    {
        var product = Product.Create(
            sku: "SKU-001",
            name: "Test Product",
            category: "Electronics",
            currentStock: 100,
            safetyStock: 20,
            leadTimeDays: 5);

        var originalUpdatedAt = product.UpdatedAt;

        Thread.Sleep(10);
        product.UpdateStock(150);

        product.CurrentStock.Should().Be(150);
        product.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}

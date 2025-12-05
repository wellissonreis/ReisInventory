namespace SmartInventoryAI.Api.DTOs.AI;

public record AISuggestionResponse(
    Guid ProductId,
    string ProductName,
    int? SuggestedQuantity,
    string Explanation,
    DateTime GeneratedAt);

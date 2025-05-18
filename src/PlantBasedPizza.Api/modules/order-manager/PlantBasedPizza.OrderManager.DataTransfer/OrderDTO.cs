namespace PlantBasedPizza.OrderManager.DataTransfer;

public record OrderDto(string OrderIdentifier, List<OrderItemDto> OrderItems);

public record OrderItemDto(string RecipeIdentifier, string ItemName, int Quantity);
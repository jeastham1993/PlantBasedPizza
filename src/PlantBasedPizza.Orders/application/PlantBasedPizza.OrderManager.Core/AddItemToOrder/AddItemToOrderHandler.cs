using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.AddItemToOrder;

public class AddItemToOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRecipeService _recipeService;
    private readonly ILogger<AddItemToOrderHandler> _logger;

    public AddItemToOrderHandler(IOrderRepository orderRepository, IRecipeService recipeService, ILogger<AddItemToOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _recipeService = recipeService;
        _logger = logger;
    }
    
    public async Task<Order?> Handle(AddItemToOrderCommand command)
    {
        try
        {
            var recipe = await this._recipeService.GetRecipe(command.RecipeIdentifier);
            
            this._logger.LogInformation("Recipe is {RecipeIdentifier} with name {RecipeName}", recipe.RecipeIdentifier, recipe.Name);
            
            var order = await this._orderRepository.Retrieve(command.CustomerIdentifier, command.OrderIdentifier);

            if (order.CustomerIdentifier != command.CustomerIdentifier)
            {
                throw new OrderNotFoundException(command.OrderIdentifier);
            } 

            order.AddOrderItem(command.RecipeIdentifier, recipe.Name, command.Quantity, recipe.Price);

            await this._orderRepository.Update(order);

            return order;
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}
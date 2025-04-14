using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.Recipes.Core.OrderCompletedHandler;

public class OrderCompletedHandler(ILogger<OrderCompletedHandler> logger, IRecipeRepository recipeRepository)
{
    public async Task Handle(OrderCompletedEventV2 evt)
    {
        logger.LogInformation("Processing message for {evt.OrderIdentifier} and customer {evt.CustomerIdentifier}");

        foreach (var item in evt.OrderItems)
        {
            logger.LogInformation("Processing recipe {item.Key} with quantity {item.Value}");
            
            var recipe = await recipeRepository.Retrieve(item.Key);

            if (recipe != null)
            {
                recipe.RecipeOrdered();

                await recipeRepository.Update(recipe);
            }
        }
    }
}
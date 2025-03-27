using PlantBasedPizza.Recipes.Core.IntegrationEvents;

namespace PlantBasedPizza.Recipes.Core.CreateRecipe;

public class CreateRecipeCommandHandler(IRecipeRepository recipeRepository, IEventPublisher eventPublisher)
{
    public async Task<RecipeDto?> Handle(CreateRecipeCommand command, CancellationToken cancellationToken = default)
    {
        var existingRecipe = await recipeRepository.Retrieve(command.RecipeIdentifier);

        if (existingRecipe != null)
        {
            return new RecipeDto(existingRecipe);
        }

        var category = command.Category switch
        {
            "pizza" => RecipeCategory.Pizza,
            "sides" => RecipeCategory.Sides,
            "drinks" => RecipeCategory.Drinks,
            _ => throw new ArgumentOutOfRangeException()
        };

        var recipe = new Recipe(category, command.RecipeIdentifier, command.Name, command.Price);

        foreach (var item in command.Ingredients)
        {
            recipe.AddIngredient(item.Name, item.Quantity);
        }

        await recipeRepository.Add(recipe);

        await eventPublisher.Publish(new RecipeCreatedEventV1()
        {
            RecipeIdentifier = recipe.RecipeIdentifier
        });

        return new RecipeDto(recipe);
    }
}
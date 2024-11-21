using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Entities;

public enum RecipeCategory
{
    Pizza,
    Sides,
    Drinks
}

public class Recipe
{
    private List<Ingredient> _ingredients;
        
    [JsonConstructor]
    private Recipe()
    {
        RecipeIdentifier = "";
        Name = "";
        _ingredients = new List<Ingredient>();
    }
        
    public Recipe(RecipeCategory category, string recipeIdentifier, string name, decimal price)
    {
        Category = category;
        RecipeIdentifier = recipeIdentifier;
        Name = name;
        Price = price;
        _ingredients = new List<Ingredient>();
    }
        
    [JsonPropertyName("recipeIdentifier")]
    public string RecipeIdentifier { get; private set; }
        
    [JsonPropertyName("category")]
    public RecipeCategory Category { get; private set; }
        
    [JsonPropertyName("name")]
    public string Name { get; private set; }
        
    [JsonPropertyName("price")]
    public decimal Price { get; private set; }

    [JsonPropertyName("ingredients")]
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients;

    public void AddIngredient(string name, int quantity)
    {
        if (_ingredients == null)
        {
            _ingredients = new List<Ingredient>();
        }
            
        _ingredients.Add(new Ingredient(name, quantity));
    }
}
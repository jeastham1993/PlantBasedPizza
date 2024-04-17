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
        this.RecipeIdentifier = "";
        this.Name = "";
        this._ingredients = new List<Ingredient>();
    }
        
    public Recipe(RecipeCategory category, string recipeIdentifier, string name, decimal price)
    {
        this.Category = category;
        this.RecipeIdentifier = recipeIdentifier;
        this.Name = name;
        this.Price = price;
        this._ingredients = new List<Ingredient>();
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
    public IReadOnlyCollection<Ingredient> Ingredients => this._ingredients;

    public void AddIngredient(string name, int quantity)
    {
        if (this._ingredients == null)
        {
            this._ingredients = new List<Ingredient>();
        }
            
        this._ingredients.Add(new Ingredient(name, quantity));
    }
}
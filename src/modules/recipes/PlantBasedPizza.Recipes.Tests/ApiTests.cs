using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.Recipes.Tests.Models;
using Xunit;

namespace PlantBasedPizza.Recipes.Tests;

public class ApiTests
{
    private const string BASE_URL = @"";
    private HttpClient _testClient;

    public ApiTests()
    {
        this._testClient = new HttpClient();
    }
    
    [Fact]
    public async Task CanListRecipes_ShouldReturnOk()
    {
        var result = await this._testClient.GetAsync($"{BASE_URL}/recipes");

        result.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Fact]
    public async Task CanGetSpecificRecipes_ShouldReturnOk()
    {
        var result = await this._testClient.GetAsync($"{BASE_URL}/recipes");

        var recipeContents = JsonSerializer.Deserialize<List<Recipe>>(result.Content.ReadAsStream());

        var recipe = recipeContents.FirstOrDefault();
        
        var getResult = await this._testClient.GetAsync($"{BASE_URL}/recipes/{recipe.RecipeIdentifier}");

        getResult.IsSuccessStatusCode.Should().BeTrue();
    }
}
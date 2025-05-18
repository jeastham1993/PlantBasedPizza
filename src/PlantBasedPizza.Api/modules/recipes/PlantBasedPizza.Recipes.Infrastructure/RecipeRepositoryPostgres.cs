using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

internal class RecipeRepositoryPostgres : IRecipeRepository
{
    private readonly ILogger<RecipeRepositoryPostgres> _logger;
    private readonly RecipesDbContext _context;

    public RecipeRepositoryPostgres(RecipesDbContext context, ILogger<RecipeRepositoryPostgres> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task<Recipe?> Retrieve(string recipeIdentifier)
    {
        return await _context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.RecipeIdentifier == recipeIdentifier);
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        return await _context
            .Recipes
            .Include(r => r.Ingredients)
            .ToListAsync();
    }

    public async Task Add(Recipe? recipe)
    {
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Recipe? recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
    }
} 
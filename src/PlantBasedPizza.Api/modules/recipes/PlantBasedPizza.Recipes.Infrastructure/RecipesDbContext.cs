// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class RecipesDbContext : DbContext
{
    public RecipesDbContext(DbContextOptions<RecipesDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes { get; set; }
    
    public DbSet<Ingredient> Ingredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .HasKey(r => r.RecipeIdentifier);
        
        modelBuilder.Entity<Recipe>()
            .ToTable("recipes")
            .HasMany(i => i.Ingredients)
            .WithOne()
            .HasForeignKey("RecipeIdentifier")
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Ingredient>()
            .HasKey(i => i.IngredientIdentifier);
    }
}
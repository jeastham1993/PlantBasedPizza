using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class ArchitectureTests
{
    [Theory]
    [InlineData("PlantBasedPizza.OrderManager.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Recipes.Core", new[] { "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Kitchen.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Deliver.Core", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure" })]
    [InlineData("PlantBasedPizza.OrderManager.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Recipes.Infrastructure", new[] { "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Kitchen.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Deliver.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure", "PlantBasedPizza.Deliver.Infrastructure" })]
    [InlineData("PlantBasedPizza.Deliver.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Recipes.Infrastructure", "PlantBasedPizza.Kitchen.Infrastructure", "PlantBasedPizza.OrderManager.Infrastructure" })]
    public void ModuleToModuleDepenedencies_ShouldOnlyBeViaDataTransferLibraries(string assemblyUnderTest, string[] compareAgainst)
    {
        foreach (var assembly in compareAgainst)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ResideInNamespace(assemblyUnderTest)
                .Should()
                .NotHaveDependencyOn(assembly)
                .GetResult()
                .IsSuccessful
                .Should()
                .BeTrue($"{assemblyUnderTest} should not have a dependency on {assembly}");
        }
    }

    [Fact]
    public void OrderManagerService_ShouldDependOnRecipeDataTransferLibrary()
    {
        var assemblyUnderTest = "PlantBasedPizza.OrderManager.Infrastructure";
        var assembly = "PlantBasedPizza.Recipes.DataTransfer";
        var implements = typeof(OrderManager.Core.Services.IRecipeService);

        var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ImplementInterface(implements)
                .Should()
                .HaveDependencyOn(assembly)
                .GetResult()
                .IsSuccessful;

        result.Should()
            .BeTrue($"{assemblyUnderTest} is expected to have a dependency on {assembly}");
    }

    [Fact]
    public void KitchenService_ShouldDependOnRecipeDataTransferLibrary()
    {
        var assemblyUnderTest = "PlantBasedPizza.Kitchen.Infrastructure";
        var assembly = "PlantBasedPizza.Recipes.DataTransfer";
        var implements = typeof(Kitchen.Core.Services.IRecipeService);

        var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ImplementInterface(implements)
                .Should()
                .HaveDependencyOn(assembly)
                .GetResult()
                .IsSuccessful;

        result.Should()
            .BeTrue($"{assemblyUnderTest} is expected to have a dependency on {assembly}");
    }

    [Fact]
    public void KitchenService_ShouldDependOnOrdersDataTransferLibrary()
    {
        var assemblyUnderTest = "PlantBasedPizza.Kitchen.Infrastructure";
        var assembly = "PlantBasedPizza.OrderManager.DataTransfer";
        var implements = typeof(Kitchen.Core.Services.IOrderManagerService);

        var result = Types.InAssembly(Assembly.Load(assemblyUnderTest))
                .That()
                .ImplementInterface(implements)
                .Should()
                .HaveDependencyOn(assembly)
                .GetResult()
                .IsSuccessful;

        result.Should()
            .BeTrue($"{assemblyUnderTest} is expected to have a dependency on {assembly}");
    }
}
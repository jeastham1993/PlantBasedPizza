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
    public void CoreLibraries_ShouldNotReferenceOtherLibraries(string assemblyUnderTest, string[] compareAgainst)
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
                .BeTrue("Core libraries should not reference other core libraries");
        }
    }
    
    [Theory]
    [InlineData("PlantBasedPizza.OrderManager.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core" })]
    [InlineData("PlantBasedPizza.Recipes.Infrastructure", new[] { "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.Deliver.Core" })]
    [InlineData("PlantBasedPizza.Kitchen.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.OrderManager.Core", "PlantBasedPizza.Deliver.Core" })]
    [InlineData("PlantBasedPizza.Deliver.Infrastructure", new[] { "PlantBasedPizza.Recipes.Core", "PlantBasedPizza.Kitchen.Core", "PlantBasedPizza.OrderManager.Core" })]
    public void InfrastructureLibraries_ShouldNotReferenceCoreLibraries(string assemblyUnderTest, string[] compareAgainst)
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
                .BeTrue($"Infrastructure library {assemblyUnderTest} should not directly reference core library {assembly}");
        }
    }
}
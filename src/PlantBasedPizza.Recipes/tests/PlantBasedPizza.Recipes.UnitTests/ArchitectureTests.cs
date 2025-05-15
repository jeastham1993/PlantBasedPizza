using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using PlantBasedPizza.Events;
using PlantBasedPizza.Recipes.Api;
using PlantBasedPizza.Recipes.Core;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PlantBasedPizza.Recipes.UnitTests;

public class ArchitectureTests
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(Recipe).Assembly;
    private static readonly System.Reflection.Assembly SharedAssembly = typeof(CorsSettings).Assembly;
    private static readonly System.Reflection.Assembly EventsAssembly = typeof(IntegrationEvent).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(BackgroundWorker).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(RecipeRepository).Assembly;

    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(CoreAssembly, SharedAssembly, EventsAssembly, ApiAssembly, InfrastructureAssembly).Build();

    private readonly IObjectProvider<IType> CoreLayer = Types()
        .That()
        .ResideInAssembly(CoreAssembly)
        .As("Core Layer");

    private readonly IObjectProvider<IType> SharedLayer = Types()
        .That()
        .ResideInAssembly(SharedAssembly)
        .As("Shared Layer");

    private readonly IObjectProvider<IType> EventsLayer = Types()
        .That()
        .ResideInAssembly(EventsAssembly)
        .As("Events Layer");

    private readonly IObjectProvider<IType> ApiLayer = Types()
        .That()
        .ResideInAssembly(ApiAssembly)
        .As("API Layer");

    private readonly IObjectProvider<IType> InfrastructureLayer = Types()
        .That()
        .ResideInAssembly(InfrastructureAssembly)
        .As("Infrastructure Layer");

    [Fact]
    public void CoreLayerShouldNotDependOnAnyOtherLayers()
    {
        IArchRule coreLayerShouldNotDependOnApplicationLayer = Types()
            .That()
            .Are(CoreLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInAssembly(ApiAssembly);
        IArchRule coreLayerShouldNotDependOnInfraLayer = Types()
            .That()
            .Are(CoreLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInAssembly(InfrastructureAssembly);

        coreLayerShouldNotDependOnApplicationLayer.Check(Architecture);
        coreLayerShouldNotDependOnInfraLayer.Check(Architecture);
    }

    [Fact]
    public void CoreLayerShouldHaveNoExternalDependencies()
    {
        var coreLayerShouldHaveNoExternalDeps = Types()
            .That()
            .Are(CoreLayer)
            .Should()
            .DependOnAnyTypesThat()
            .ResideInAssembly(CoreAssembly)
            .Or(Types()
                .That()
                .Are(CoreLayer)
                .Should()
                .OnlyDependOnTypesThat().ResideInNamespace("System.*")
            .Or(Types()
                .That()
                .Are(CoreLayer)
                .Should()
                .OnlyDependOnTypesThat()
                .ResideInAssembly(EventsAssembly)));

        coreLayerShouldHaveNoExternalDeps.Check(Architecture);
    }
}
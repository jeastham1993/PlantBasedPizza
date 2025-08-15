using System.Diagnostics;
using PlantBasedPizza.FitnessFunctions.Drivers;

namespace PlantBasedPizza.FitnessFunctions;

public class Tests
{
    [Test]
    public async Task TestRecipeApiPerformance_ShouldRespondInside200ms()
    {
        var recipeDriver = new RecipeDriver();
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        var response = await recipeDriver.All();
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        Assert.That(elapsedMilliseconds, Is.LessThan(200),
            $"API response took too long: {elapsedMilliseconds} ms");
    }
}
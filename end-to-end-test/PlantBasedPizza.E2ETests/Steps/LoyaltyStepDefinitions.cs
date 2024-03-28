using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps;

[Binding]
public class LoyaltyStepDefinitions
{
    private readonly LoyaltyPointsDriver _driver = new();

    [Then(@"the total points should be greater than (.*) for (.*)")]
    public async Task ThenTheTotalPointsShouldBe(int totalPoints, string customerIdentifier)
    {
        var points = await this._driver.GetLoyaltyPoints(customerIdentifier);

        points.TotalPoints.Should().BeGreaterThan(totalPoints);
    }
}
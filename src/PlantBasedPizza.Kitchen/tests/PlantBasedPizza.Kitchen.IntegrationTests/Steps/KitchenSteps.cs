using FluentAssertions;
using PlantBasedPizza.Kitchen.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Kitchen.IntegrationTests.Steps;

[Binding]
public class KitchenSteps
{
    private readonly KitchenDriver _driver;

    public KitchenSteps()
    {
        this._driver = new KitchenDriver();
    }
    
    [Given(@"a new order submitted event is raised for order (.*)")]
    public async Task GivenANewOrderSubmittedEventIsRaisedForOrderOrd(string p0)
    {
        await this._driver.NewOrderSubmitted(p0);
    }


    [Then(@"order (.*) should appear in the kitchen new order list")]
    public async Task ThenOrderOrdShouldAppearInTheKitchenNewOrderList(string p0)
    {
        var newOrders = await this._driver.GetNewOrders();

        newOrders.Any(req => req.OrderIdentifier.Equals(p0, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }
}
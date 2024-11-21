using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Account.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Account.IntegrationTests.Steps;

[Binding]
public class AccountStepDefinitions
{
    private readonly AccountDriver _driver;
    private readonly ScenarioContext _scenarioContext;

    public AccountStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new AccountDriver();
    }
    [Given("an un-registered email address")]
    public void AnUnregisteredEmail()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        _scenarioContext.Add("emailAddress", "arandomemail@test.com");
        _scenarioContext.Add("password", "RandomPassword!23");
    }

    [Given(@"a user registers")]
    public async Task AUserRegisters()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var emailAddress = $"{Guid.NewGuid().ToString()}@test.com";
        _scenarioContext.Add("emailAddress", emailAddress);
        
        var password = $"{Guid.NewGuid()}!A23";
        _scenarioContext.Add("password", password);

        var res = await _driver.RegisterUser(emailAddress, password);

        res.Should().NotBeNull();
    }

    [Then(@"they should be able to successfully login")]
    public async Task TheyShouldBeAbleToLogin()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var emailAddress = _scenarioContext.Get<string>("emailAddress");
        var password = _scenarioContext.Get<string>("password");
            
        var loginResponse = await _driver.Login(emailAddress, password);

        loginResponse.Should().NotBeNull();
        loginResponse!.AuthToken.Should().NotBeEmpty();
    }

    [Given(@"an invalid email address")]
    public void AnInvalidEmailAddress()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var emailAddress = $"{Guid.NewGuid().ToString()}emfoiwenfwfe";
        var password = "AValidPassword!23";
        _scenarioContext.Add("emailAddress", emailAddress);
        _scenarioContext.Add("password", password);
    }

    [Given(@"an invalid password")]
    public void AnInvalidPassword()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var emailAddress = $"{Guid.NewGuid().ToString()}@test.com";
        var password = "1234";
        
        _scenarioContext.Add("emailAddress", emailAddress);
        _scenarioContext.Add("password", password);
    }

    [Given(@"an empty email address")]
    public void AnEmptyEmailAddress()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var emailAddress = $"";
        var password = "AValidPassword!23";
        _scenarioContext.Add("emailAddress", emailAddress);
        _scenarioContext.Add("password", password);
    }

    [Given(@"an empty password")]
    public void AnEmptyPassword()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var emailAddress = $"{Guid.NewGuid().ToString()}@test.com";
        var password = "";
        
        _scenarioContext.Add("emailAddress", emailAddress);
        _scenarioContext.Add("password", password);
    }
    
    [Then("they should not be able to login with an invalid password")]
    public async Task ThenTheyShouldNotLoginWithAnInvalidPassword()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var emailAddress = _scenarioContext.Get<string>("emailAddress");
            
        var loginResponse = await _driver.Login(emailAddress, "some random stuff");

        loginResponse.Should().BeNull();
    }
    
    [Then("they should not be able to login")]
    public async Task ThenTheyCantLogin()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var emailAddress = _scenarioContext.Get<string>("emailAddress");
        var password = _scenarioContext.Get<string>("password");
            
        var loginResponse = await _driver.Login(emailAddress, "some random stuff");

        loginResponse.Should().BeNull();
    }

    [Then("registration should fail")]
    public async Task RegistrationShouldFail()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var emailAddress = _scenarioContext.Get<string>("emailAddress");
        var password = _scenarioContext.Get<string>("password");

        var registerResult = await _driver.RegisterUser(emailAddress, password);

        registerResult.Should().BeNull();
    }
}
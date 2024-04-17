namespace PlantBasedPizza.Account.Api.Core;

public class LoginCommand
{
    public string EmailAddress { get; set; }
    
    public string Password { get; set; }
}
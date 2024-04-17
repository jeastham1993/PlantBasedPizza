namespace PlantBasedPizza.Account.Api.Core;

public class RegisterUserCommand
{
    public string EmailAddress { get; set; }
    
    public string Password { get; set; }
}
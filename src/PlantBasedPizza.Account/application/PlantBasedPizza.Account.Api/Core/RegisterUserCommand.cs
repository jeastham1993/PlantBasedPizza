namespace PlantBasedPizza.Account.Api.Core;

public class RegisterUserCommand
{
    public string EmailAddress { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}
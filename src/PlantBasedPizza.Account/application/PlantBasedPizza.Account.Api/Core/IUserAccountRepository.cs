namespace PlantBasedPizza.Account.Api.Core;

public interface IUserAccountRepository
{
    Task<UserAccount> CreateAccount(string emailAddress, string password);

    Task<UserAccount> ValidateCredentials(string emailAddress, string password);
}
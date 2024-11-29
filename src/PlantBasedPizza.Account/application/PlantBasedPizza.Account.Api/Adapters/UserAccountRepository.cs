using MongoDB.Driver;
using PlantBasedPizza.Account.Api.Core;

namespace PlantBasedPizza.Account.Api.Adapters;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly IMongoCollection<UserAccount> _accounts;

    public UserAccountRepository(MongoClient client)
    {
        var database = client.GetDatabase("PlantBasedPizza-Accounts");
        _accounts = database.GetCollection<UserAccount>("accounts");
    }
    
    public async Task<UserAccount> CreateAccount(UserAccount userAccount)
    {
        var queryBuilder = Builders<UserAccount>.Filter.Eq(p => p.EmailAddress, userAccount.EmailAddress);

        var existingAccount = await _accounts.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        if (existingAccount is not null)
        {
            throw new UserExistsException();
        }
        
        await _accounts.InsertOneAsync(userAccount).ConfigureAwait(false);

        return userAccount;
    }

    public async Task<UserAccount> ValidateCredentials(string emailAddress, string password)
    {
        var queryBuilder = Builders<UserAccount>
            .Filter;

        var filter = queryBuilder.Eq(account => account.EmailAddress, emailAddress) &
                     queryBuilder.Eq(account => account.Password, UserAccount.HashPassword(password));

        var account = await _accounts.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

        if (account == null)
        {
            throw new LoginFailedException();
        }

        return account;
    }

    public async Task SeedInitialUser()
    {
        try
        {
            await CreateAccount(UserAccount.Create("admin@plantbasedpizza.com", "AdminAccount!23", AccountType.Admin));
        }
        catch (UserExistsException)
        {
            // There is no problem if this fails.
        }
    }
}
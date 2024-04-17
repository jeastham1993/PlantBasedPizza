using System.Security.Cryptography;
using System.Text;
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
    
    public async Task<UserAccount> CreateAccount(string emailAddress, string password)
    {
        return await this.InsertUserAccount(emailAddress, password, AccountType.User);
    }

    public async Task<UserAccount> CreateStaffAccount(string emailAddress, string password)
    {
        if (!emailAddress.EndsWith("@plantbasedpizza.com"))
        {
            throw new InvalidUserException();
        }

        return await this.InsertUserAccount(emailAddress, password, AccountType.Staff);
    }

    public async Task<UserAccount> CreateDriverAccount(string emailAddress, string password)
    {
        return await this.InsertUserAccount(emailAddress, password, AccountType.Driver);
    }

    public async Task<UserAccount> ValidateCredentials(string emailAddress, string password)
    {
        var queryBuilder = Builders<UserAccount>
            .Filter;

        var filter = queryBuilder.Eq(account => account.EmailAddress, emailAddress) &
                     queryBuilder.Eq(account => account.Password, HashPassword(password));

        var account = await this._accounts.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

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
            await InsertUserAccount("admin@plantbasedpizza.com", "Admin!23", AccountType.Admin);
        }
        catch (UserExistsException){}
    }

    // Note: This hashing algorithm may not be suitable for production scenarios with real user data
    private static string HashPassword(string password)
    {
        // Create a new instance of the SHA512 hash algorithm
        using SHA512 sha512Hash = SHA512.Create();
        // Convert the input string to a byte array and compute the hash
        byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Create a new Stringbuilder to collect the bytes
        // and create a string
        StringBuilder builder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string
        for (int i = 0; i < data.Length; i++)
        {
            builder.Append(data[i].ToString("x2"));
        }

        // Return the hashed string
        return builder.ToString();
    }

    private async Task<UserAccount> InsertUserAccount(string emailAddress, string password, AccountType type)
    {
        var queryBuilder = Builders<UserAccount>.Filter.Eq(p => p.EmailAddress, emailAddress);

        var account = await this._accounts.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        if (account is not null)
        {
            throw new UserExistsException();
        }
        
        var userAccount = new UserAccount()
        {
            AccountId = Guid.NewGuid().ToString(),
            EmailAddress = emailAddress,
            Password = HashPassword(password),
            AccountType = type
        };
        
        await _accounts.InsertOneAsync(userAccount).ConfigureAwait(false);

        return userAccount;
    }
}
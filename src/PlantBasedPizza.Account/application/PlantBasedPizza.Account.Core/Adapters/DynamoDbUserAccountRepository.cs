using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Account.Core.Adapters;

public class DynamoDbUserAccountRepository : IUserAccountRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;
    
    public DynamoDbUserAccountRepository(IAmazonDynamoDB dynamoDbClient, IOptions<DynamoDbOptions> options)
    {
        _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
        _dynamoDbClient = dynamoDbClient;
        _tableName = options.Value.TableName;
    }

    public async Task<UserAccount> CreateAccount(UserAccount userAccount)
    {
        // Check if account with email already exists using GetItem
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = userAccount.EmailAddress } }
            }
        };

        var existingAccount = await _dynamoDbClient.GetItemAsync(getItemRequest).ConfigureAwait(false);

        if (existingAccount.Item != null && existingAccount.Item.Count > 0)
        {
            throw new UserExistsException();
        }

        // Save the new account to DynamoDB with email as PK
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = userAccount.EmailAddress } },
            { "AccountId", new AttributeValue { S = userAccount.AccountId } },
            { "EmailAddress", new AttributeValue { S = userAccount.EmailAddress } },
            { "Password", new AttributeValue { S = userAccount.Password } },
            { "AccountType", new AttributeValue { S = userAccount.AccountType.ToString() } }
        };

        await _dynamoDbClient.PutItemAsync(_tableName, item).ConfigureAwait(false);
        
        return userAccount;
    }

    public async Task<UserAccount> ValidateCredentials(string emailAddress, string password)
    {
        // Get the user by email address
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = emailAddress } }
            }
        };

        var result = await _dynamoDbClient.GetItemAsync(getItemRequest).ConfigureAwait(false);

        if (result.Item == null || result.Item.Count == 0)
        {
            throw new LoginFailedException();
        }

        // Verify the password
        var storedPassword = result.Item["Password"].S;
        var hashedInputPassword = UserAccount.HashPassword(password);

        if (storedPassword != hashedInputPassword)
        {
            throw new LoginFailedException();
        }

        // Convert DynamoDB item to UserAccount object
        var account = new UserAccount
        {
            AccountId = result.Item["AccountId"].S,
            EmailAddress = result.Item["EmailAddress"].S,
            Password = result.Item["Password"].S,
            AccountType = (AccountType)Enum.Parse(typeof(AccountType), result.Item["AccountType"].S)
        };

        return account;
    }

    public async Task SeedInitialUser()
    {
        try
        {
            await CreateAccount(UserAccount.Create("admin@plantbasedpizza.com", "AdminAccount!23", AccountType.Admin));
        }
        catch (UserExistsException) { }
    }
}
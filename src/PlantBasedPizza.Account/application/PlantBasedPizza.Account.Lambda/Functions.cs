using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using PlantBasedPizza.Account.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PlantBasedPizza.Account.Lambda;

/// <summary>
/// Lambda functions that provide the same API as the PlantBasedPizza.Account.Api
/// </summary>
public class Functions
{
    private readonly IUserAccountRepository _accountRepository;
    private readonly UserAccountService _userAccountService;
    private readonly ILogger<Functions> _logger;

    public Functions(
        IUserAccountRepository accountRepository,
        UserAccountService userAccountService,
        ILogger<Functions> logger)
    {
        _accountRepository = accountRepository;
        _userAccountService = userAccountService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/account/health")]
    public async Task<string> Health(ILambdaContext context)
    {
        context.Logger.LogInformation("Health check requested");
        return await Task.FromResult("OK");
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/account/login")]
    public async Task<IHttpResult> Login([FromBody] LoginCommand login, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Login attempt for {login.EmailAddress}");
            var loginResponse = await _userAccountService.Login(login);
            return HttpResults.Ok(loginResponse);
        }
        catch (LoginFailedException)
        {
            context.Logger.LogWarning($"Login failed for {login.EmailAddress}");
            return HttpResults.Unauthorized();
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/account/register")]
    public async Task<IHttpResult> RegisterUser([FromBody] RegisterUserCommand register, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"User registration attempt for {register.EmailAddress}");
            var userAccount = await _userAccountService.Register(register, AccountType.User);
            return HttpResults.Ok(new RegisterResponse { AccountId = userAccount.AccountId });
        }
        catch (UserExistsException)
        {
            context.Logger.LogWarning($"Registration failed - user exists: {register.EmailAddress}");
            return HttpResults.BadRequest("User exists");
        }
        catch (InvalidUserException ex)
        {
            context.Logger.LogWarning($"Registration failed - invalid user: {ex.Reason}");
            return HttpResults.BadRequest(ex.Reason);
        }
    }

    /// <summary>
    /// Register a new driver account
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/account/driver/register")]
    public async Task<IHttpResult> RegisterDriver([FromBody] RegisterUserCommand register, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Driver registration attempt for {register.EmailAddress}");
            var userAccount = await _userAccountService.Register(register, AccountType.Driver);
            return HttpResults.Ok(new RegisterResponse { AccountId = userAccount.AccountId });
        }
        catch (UserExistsException)
        {
            context.Logger.LogWarning($"Registration failed - user exists: {register.EmailAddress}");
            return HttpResults.BadRequest("User exists");
        }
        catch (InvalidUserException ex)
        {
            context.Logger.LogWarning($"Registration failed - invalid user: {ex.Reason}");
            return HttpResults.BadRequest(ex.Reason);
        }
    }

    /// <summary>
    /// Register a new staff account - requires admin role
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/account/staff/register")]
    public async Task<IHttpResult> RegisterStaff([FromBody] RegisterUserCommand register, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Staff registration attempt for {register.EmailAddress}");
            var userAccount = await _userAccountService.Register(register, AccountType.Staff);
            return HttpResults.Ok(new RegisterResponse { AccountId = userAccount.AccountId });
        }
        catch (UserExistsException)
        {
            context.Logger.LogWarning($"Registration failed - user exists: {register.EmailAddress}");
            return HttpResults.BadRequest("User exists");
        }
        catch (InvalidUserException ex)
        {
            context.Logger.LogWarning($"Registration failed - invalid user: {ex.Reason}");
            return HttpResults.BadRequest(ex.Reason);
        }
    }

    /// <summary>
    /// Initialize the service by seeding the initial admin user
    /// </summary>
    [LambdaFunction]
    public async Task Initialize(ILambdaContext context)
    {
        context.Logger.LogInformation("Initializing account service - seeding admin user");
        await _accountRepository.SeedInitialUser();
        context.Logger.LogInformation("Initialization complete");
    }
}
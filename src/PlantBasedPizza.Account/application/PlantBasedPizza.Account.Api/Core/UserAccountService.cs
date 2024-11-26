using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PlantBasedPizza.Account.Api.Core;

public class UserAccountService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly JwtConfiguration _configuration;

    public UserAccountService(IUserAccountRepository userAccountRepository, IOptions<JwtConfiguration> configuration)
    {
        _userAccountRepository = userAccountRepository;
        _configuration = configuration.Value;
    }

    public async Task<LoginResponse> Login(LoginCommand request)
    {
        try
        {
            var account = await _userAccountRepository.ValidateCredentials(request.EmailAddress, request.Password);
    
            var issuer = _configuration.Issuer;
            var audience = _configuration.Audience;
            var key = Encoding.ASCII.GetBytes
                (_configuration.Key);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.AccountId),
                new Claim(JwtRegisteredClaimNames.Email, account.EmailAddress),
                new Claim(ClaimTypes.Role, account.AsAuthenticatedRole()),
                new Claim("UserType", account.AccountType.ToString()),
                new Claim("UserTier", account.AccountTier.ToString()),
                new Claim("AccountAge", account.AccountAge.ToString(CultureInfo.InvariantCulture)),
            };
            
            Activity.Current?.AddTag("user.type", account.AccountType.ToString());
            Activity.Current?.AddTag("user.tier", account.AccountTier.ToString());
            Activity.Current?.AddTag("user.account_age", account.AccountAge);
        
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
        
            var stringToken = tokenHandler.WriteToken(token);

            return new LoginResponse()
            {
                AuthToken = stringToken
            };
        }
        catch (LoginFailedException ex)
        {
            Activity.Current?.AddTag("login.failed", true);
            Activity.Current?.AddException(ex);

            throw;
        }
    }

    public async Task<RegisterResponse> Register(RegisterUserCommand request, AccountType accountType)
    {
        try
        {
            UserAccount? userAccount = null;
            
            switch (accountType)
            {
                case AccountType.User:
                    userAccount = UserAccount.Create(request.EmailAddress, request.Password, AccountType.User);
                    break;
                case AccountType.Driver:
                    userAccount = UserAccount.Create(request.EmailAddress, request.Password, AccountType.Driver);
                    break;
                case AccountType.Staff:
                    if (!request.EmailAddress.EndsWith("@plantbasedpizza.com"))
                    {
                        throw new InvalidUserException("Not a valid staff email");
                    }
                    
                    userAccount = UserAccount.Create(request.EmailAddress, request.Password, AccountType.Staff);
                    break;
            }

            await _userAccountRepository.CreateAccount(userAccount);
            
            return new RegisterResponse()
            {
                AccountId = userAccount.AccountId
            };
        }
        catch (UserExistsException ex)
        {
            Activity.Current?.AddTag("user.exists", true);
            Activity.Current?.AddException(ex);

            throw;
        }
    }
}
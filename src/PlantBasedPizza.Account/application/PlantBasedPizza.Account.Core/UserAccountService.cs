using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PlantBasedPizza.Account.Core;

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

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.AccountId),
                new Claim(JwtRegisteredClaimNames.Email, account.EmailAddress),
                new Claim(ClaimTypes.Role, account.AsAuthenticatedRole())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = _configuration.Issuer,
                Audience = _configuration.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.Key)),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse { AuthToken = tokenHandler.WriteToken(token) };
        }
        catch (LoginFailedException)
        {
            throw;
        }
    }

    public async Task<RegisterResponse> Register(RegisterUserCommand request, AccountType accountType)
    {
        var userAccount = UserAccount.Create(request.EmailAddress, request.Password, accountType);
        await _userAccountRepository.CreateAccount(userAccount);

        return new RegisterResponse { AccountId = userAccount.AccountId };
    }
}

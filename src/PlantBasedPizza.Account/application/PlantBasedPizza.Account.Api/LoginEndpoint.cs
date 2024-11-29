using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using PlantBasedPizza.Account.Api.Core;

namespace PlantBasedPizza.Account.Api;

[HttpPost("/login")]
[AllowAnonymous]
public class LoginEndpoint(ILogger<LoginEndpoint> logger, UserAccountService userAccountService)
    : Endpoint<LoginCommand, LoginResponse?>
{
    public override async Task<LoginResponse?> HandleAsync(
        LoginCommand request,
        CancellationToken ct)
    {
        try
        {
            var loginResponse = await userAccountService.Login(request);
            
            Response = loginResponse;
            return loginResponse;
        }
        catch (LoginFailedException ex)
        {
            logger.LogError(ex, "Failed to login");
            await SendErrorsAsync(400, ct);
            return null;
        }
    }
}
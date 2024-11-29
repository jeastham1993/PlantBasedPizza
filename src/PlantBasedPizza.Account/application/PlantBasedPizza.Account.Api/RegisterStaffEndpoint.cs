using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using PlantBasedPizza.Account.Api.Core;

namespace PlantBasedPizza.Account.Api;

[HttpPost("/staff/register")]
[AllowAnonymous]
[Authorize(Roles = "admin")]
public class RegisterStaffEndpoint(UserAccountService userAccountService, ILogger<RegisterStaffEndpoint> logger)
    : Endpoint<RegisterUserCommand, RegisterResponse?>
{
    public override async Task<RegisterResponse?> HandleAsync(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        try
        {
            var registerResponse = await userAccountService.Register(request, AccountType.Staff);

            Response = registerResponse;
            return registerResponse;
        }
        catch (UserExistsException ex)
        {
            logger.LogError(ex, "Failed to register driver");
            await SendErrorsAsync(400, ct);
            return null;
        }
    }
}
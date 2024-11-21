using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using PlantBasedPizza.Account.Api.Core;

namespace PlantBasedPizza.Account.Api;

[HttpPost("/account/staff/register")]
[AllowAnonymous]
[Authorize(Roles = "admin")]
public class RegisterStaffEndpoint(UserAccountService userAccountService, ILogger<RegisterStaffEndpoint> logger)
    : Endpoint<RegisterUserCommand, RegisterResponse?>
{
    public override async Task<RegisterResponse?> HandleAsync(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
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
            await SendErrorsAsync(400, cancellationToken);
            return null;
        }
    }
}
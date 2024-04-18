using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PlantBasedPizza.IntegrationTest.Helpers;

public static class TestTokenGenerator
{
    public static string GenerateTestTokenForRole(string roleName)
    {
        var accountId = $"{roleName}-account";
            
        var userClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId),
            new Claim(JwtRegisteredClaimNames.Email, $"{accountId}@test.com"),
            new Claim(ClaimTypes.Role, roleName)
        };
            
        var userToken = JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(
            new JwtSecurityToken(
                JwtTokenProvider.Issuer,
                JwtTokenProvider.Issuer,
                userClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: JwtTokenProvider.SigningCredentials
            )
        );

        return userToken;
    }
}
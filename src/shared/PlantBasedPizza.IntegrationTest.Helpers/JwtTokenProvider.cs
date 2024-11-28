using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace PlantBasedPizza.IntegrationTest.Helpers;

public static class JwtTokenProvider
{
    public static string Issuer { get; } = "https://plantbasedpizza.com";
    public static SecurityKey SecurityKey { get; } =
        new SymmetricSecurityKey("This is a sample secret key - please don't use in production environment.'"u8.ToArray());
    public static SigningCredentials SigningCredentials { get; } = new(SecurityKey, SecurityAlgorithms.HmacSha256);
    public static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
}
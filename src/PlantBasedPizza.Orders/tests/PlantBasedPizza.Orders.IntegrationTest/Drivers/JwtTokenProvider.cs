using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PlantBasedPizza.Orders.IntegrationTest.Drivers;

public static class JwtTokenProvider
{
    public static string Issuer { get; } = "https://plantbasedpizza.com";
    public static SecurityKey SecurityKey { get; } =
        new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes("This is a sample secret key - please don't use in production environment.'")
        );
    public static SigningCredentials SigningCredentials { get; } =
        new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    internal static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
}
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PlantBasedPizza.Account.Api.Configurations;

public static class AuthConfig
{
    public static IServiceCollection AddAuthConfigs(this IServiceCollection services,
        ILogger logger, WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Auth:Issuer"],
                ValidAudience = builder.Configuration["Auth:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"] ?? "")),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };
        });
        
        logger.LogInformation("Added auth configuration");
        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("admin", x => x.RequireRole("admin"));
            options.AddPolicy("staff", x => x.RequireRole("staff"));
        });
        
        builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Auth"));
        
        logger.LogInformation("Added auth configuration");
        
        return services;
    }
}
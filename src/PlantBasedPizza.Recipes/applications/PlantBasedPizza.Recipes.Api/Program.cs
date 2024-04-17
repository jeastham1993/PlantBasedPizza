using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Events;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

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
            (Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

var applicationName = "RecipesApi";

builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessaging(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var recipeRepo = app.Services.GetRequiredService<IRecipeRepository>();
await recipeRepo.SeedRecipes();

app.MapControllers();

app.Run();

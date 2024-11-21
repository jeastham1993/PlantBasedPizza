using System.Text.Json.Serialization;

namespace PlantBasedPizza.Account.IntegrationTests.ViewModels;

public class LoginResponse
{
    [JsonPropertyName("authToken")] public string AuthToken { get; set; } = "";
}
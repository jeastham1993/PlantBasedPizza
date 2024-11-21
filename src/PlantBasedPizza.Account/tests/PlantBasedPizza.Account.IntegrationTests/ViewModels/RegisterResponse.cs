using System.Text.Json.Serialization;

namespace PlantBasedPizza.Account.IntegrationTests.ViewModels;

public class RegisterResponse
{
    [JsonPropertyName("accountId")] public string AccountId { get; set; } = "";
}
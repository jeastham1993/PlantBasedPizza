namespace PlantBasedPizza.Payments.InMemoryTests;

public record VerificationOptions(string OrderIdentifier, bool VerifyTelemetry = true);
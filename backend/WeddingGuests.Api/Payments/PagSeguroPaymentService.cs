namespace WeddingGuests.Api.Payments;

public sealed record PagSeguroIntegrationStatus(
    string Provider,
    string Environment,
    bool Enabled,
    bool HasAccessToken,
    string CheckoutStatus);

public sealed class PagSeguroPaymentService(IConfiguration configuration)
{
    public PagSeguroIntegrationStatus GetStatus()
    {
        var environment = configuration["PAGSEGURO_ENVIRONMENT"];
        var normalizedEnvironment = string.Equals(environment, "production", StringComparison.OrdinalIgnoreCase)
            ? "production"
            : "sandbox";
        var hasAccessToken = !string.IsNullOrWhiteSpace(configuration["PAGSEGURO_ACCESS_TOKEN"]);
        var enabled = bool.TryParse(configuration["PAGSEGURO_ENABLED"], out var configuredEnabled)
            && configuredEnabled
            && hasAccessToken;

        return new PagSeguroIntegrationStatus(
            "PagSeguro/PagBank",
            normalizedEnvironment,
            enabled,
            hasAccessToken,
            "planned");
    }
}

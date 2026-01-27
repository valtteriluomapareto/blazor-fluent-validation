namespace App.Contracts;

public sealed class PrefillIntegrationDemoPrefillData
{
    public string AddressLine1 { get; init; } = string.Empty;
    public string AddressLine2 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public sealed record PrefillIntegrationDemoLookupResponse(
    bool Found,
    string LookupName,
    string MatchingName,
    PrefillIntegrationDemoPrefillData? Data,
    string Message
);

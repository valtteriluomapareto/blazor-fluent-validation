using App.Abstractions;
using App.Contracts;

namespace App.Integrations;

public sealed class MockPrefillIntegrationLookup : IPrefillIntegrationLookup
{
    private static readonly PrefillIntegrationDemoPrefillData MatchingData = new()
    {
        AddressLine1 = "123 Analytical Engine Way",
        AddressLine2 = "Suite 42",
        City = "London",
        PostalCode = "SW1A 1AA",
        PhoneNumber = "+44 20 7946 0958",
        Email = "ada.lovelace@example.com",
    };

    public async Task<PrefillIntegrationDemoPrefillData?> LookupAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        // Simulate a small amount of integration latency for the demo flow.
        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return string.Equals(
            name.Trim(),
            PrefillIntegrationDemoDefaults.MatchingName,
            StringComparison.OrdinalIgnoreCase
        )
            ? MatchingData
            : null;
    }
}

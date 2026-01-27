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

    private static readonly PrefillIntegrationDemoPrefillData SlowRaceData = new()
    {
        AddressLine1 = "123 Slow St",
        AddressLine2 = "Suite 1",
        City = "Slowville",
        PostalCode = "11111",
        PhoneNumber = "+1 111 111 1111",
        Email = "slow@example.com",
    };

    private static readonly PrefillIntegrationDemoPrefillData FastRaceData = new()
    {
        AddressLine1 = "456 Rapid Ave",
        AddressLine2 = "Floor 9",
        City = "Velocity City",
        PostalCode = "99999",
        PhoneNumber = "+1 999 999 9999",
        Email = "fast@example.com",
    };

    public async Task<PrefillIntegrationDemoPrefillData?> LookupAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var normalizedName = name.Trim();

        if (
            string.Equals(
                normalizedName,
                PrefillIntegrationDemoDefaults.SlowRaceName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            // Intentionally ignore cancellation to simulate a stale response arriving late.
            await Task.Delay(TimeSpan.FromMilliseconds(900));
            return SlowRaceData;
        }

        if (
            string.Equals(
                normalizedName,
                PrefillIntegrationDemoDefaults.FastRaceName,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            await Task.Delay(TimeSpan.FromMilliseconds(75), cancellationToken);
            return FastRaceData;
        }

        // Simulate a small amount of integration latency for the normal demo flow.
        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

        return string.Equals(
            normalizedName,
            PrefillIntegrationDemoDefaults.MatchingName,
            StringComparison.OrdinalIgnoreCase
        )
            ? MatchingData
            : null;
    }
}

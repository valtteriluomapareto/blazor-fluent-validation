using App.Contracts;

namespace App.Abstractions;

public interface IPrefillIntegrationLookup
{
    Task<PrefillIntegrationDemoPrefillData?> LookupAsync(
        string name,
        CancellationToken cancellationToken = default
    );
}

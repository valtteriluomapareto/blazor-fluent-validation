using App.Abstractions;

namespace App.Integrations;

public sealed class MockUsedNameLookup : IUsedNameLookup
{
    private static readonly string[] UsedNames = ["Taken", "Existing", "AlreadyUsed"];

    public Task<IReadOnlyCollection<string>> GetUsedNamesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult<IReadOnlyCollection<string>>(UsedNames);
    }
}

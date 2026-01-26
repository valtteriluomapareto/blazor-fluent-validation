using App.Abstractions;

namespace FormValidationTest.Client.Services;

public sealed class LocalUsedNameLookup : IUsedNameLookup
{
    public Task<IReadOnlyCollection<string>> GetUsedNamesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
    }
}

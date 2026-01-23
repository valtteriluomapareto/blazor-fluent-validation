namespace App.Abstractions;

public interface IUsedNameLookup
{
    Task<IReadOnlyCollection<string>> GetUsedNamesAsync(
        CancellationToken cancellationToken = default
    );
}

using FormValidationTest.Client.Services;

namespace App.Ui.Client.Tests;

public sealed class LocalUsedNameLookupTests
{
    [Fact]
    public async Task GetUsedNamesAsync_returns_an_empty_collection()
    {
        var lookup = new LocalUsedNameLookup();

        var result = await lookup.GetUsedNamesAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }
}

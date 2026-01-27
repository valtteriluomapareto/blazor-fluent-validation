namespace App.E2E.Tests;

[CollectionDefinition("E2E", DisableParallelization = true)]
public sealed class E2ECollection
    : ICollectionFixture<AppHostFixture>,
        ICollectionFixture<PlaywrightFixture> { }

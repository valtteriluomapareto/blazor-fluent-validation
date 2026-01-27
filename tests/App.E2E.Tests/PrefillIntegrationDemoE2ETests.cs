using App.Contracts;
using Microsoft.Playwright;
using Xunit;
using Assertions = Microsoft.Playwright.Assertions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class PrefillIntegrationDemoE2ETests
{
    private readonly AppHostFixture host;
    private readonly PlaywrightFixture playwright;
    private readonly ITestOutputHelper output;

    public PrefillIntegrationDemoE2ETests(
        AppHostFixture host,
        PlaywrightFixture playwright,
        ITestOutputHelper output
    )
    {
        this.host = host;
        this.playwright = playwright;
        this.output = output;
    }

    [Fact]
    public async Task Matching_name_prefills_form_and_validates()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/prefill-integration-demo");
        await NavigateAndWaitForWasmAsync(page, "/prefill-integration-demo");

        await Assertions
            .Expect(
                page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Prefill From Integration" }
                )
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "enter matching name");
        await FillAndCommitAsync(
            page.GetByLabel("Name"),
            PrefillIntegrationDemoDefaults.MatchingName
        );

        await Assertions
            .Expect(page.GetByLabel("Address line 1"))
            .ToHaveValueAsync("123 Analytical Engine Way", new() { Timeout = 15000 });

        await Assertions
            .Expect(page.GetByLabel("Email"))
            .ToHaveValueAsync("ada.lovelace@example.com", new() { Timeout = 15000 });

        TestReporter.Step(output, "submit prefilled form");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate form" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByText("Form is valid and ready to submit.", new() { Exact = true }))
            .ToBeVisibleAsync(new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Newer_name_wins_over_late_stale_response()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/prefill-integration-demo");
        await NavigateAndWaitForWasmAsync(page, "/prefill-integration-demo");

        await Assertions
            .Expect(
                page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Prefill From Integration" }
                )
            )
            .ToBeVisibleAsync();

        var nameInput = page.GetByLabel("Name");

        TestReporter.Step(output, "trigger slow lookup then fast lookup");
        await FillAndCommitAsync(nameInput, PrefillIntegrationDemoDefaults.SlowRaceName);
        await FillAndCommitAsync(nameInput, PrefillIntegrationDemoDefaults.FastRaceName);

        await Assertions
            .Expect(page.GetByLabel("Address line 1"))
            .ToHaveValueAsync("456 Rapid Ave", new() { Timeout = 15000 });

        // Wait long enough for the slow stale response to arrive and confirm it is ignored.
        await page.WaitForTimeoutAsync(1200);

        await Assertions
            .Expect(page.GetByLabel("Address line 1"))
            .ToHaveValueAsync("456 Rapid Ave", new() { Timeout = 15000 });

        await Assertions
            .Expect(page.GetByLabel("Email"))
            .ToHaveValueAsync("fast@example.com", new() { Timeout = 15000 });
    }

    private static async Task NavigateAndWaitForWasmAsync(IPage page, string path)
    {
        var wasmResponse = page.WaitForResponseAsync(response =>
            response.Url.Contains("/_framework/dotnet", StringComparison.OrdinalIgnoreCase)
            && response.Status == 200
        );

        await page.GotoAsync(
            path,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
        );
        await wasmResponse;
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task FillAndCommitAsync(ILocator locator, string value)
    {
        await locator.FillAsync(value);
        await locator.EvaluateAsync(
            "el => el.dispatchEvent(new Event('change', { bubbles: true }))"
        );
    }
}

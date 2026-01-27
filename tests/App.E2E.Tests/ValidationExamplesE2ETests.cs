using Microsoft.Playwright;
using Xunit;
using Assertions = Microsoft.Playwright.Assertions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class ValidationExamplesE2ETests
{
    private readonly AppHostFixture host;
    private readonly PlaywrightFixture playwright;
    private readonly ITestOutputHelper output;

    public ValidationExamplesE2ETests(
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
    public async Task Validation_examples_submits_when_required_fields_are_valid()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/validation-examples");
        await NavigateAndWaitForWasmAsync(page, "/validation-examples");

        await Assertions
            .Expect(
                page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Validation Examples" }
                )
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "fill required text inputs");
        await FillAndCommitAsync(page.GetByLabel("Required SSN"), "010199-8148");
        await FillAndCommitAsync(page.GetByLabel("Required business ID"), "2617416-4");
        await FillAndCommitAsync(page.GetByLabel("Required IBAN"), "NL91 ABNA 0417 1643 00");
        await FillAndCommitAsync(page.GetByLabel("Required email"), "name@example.com");
        await FillAndCommitAsync(page.GetByLabel("Required decimal (fi-FI)"), "1234.56");
        await FillAndCommitAsync(page.GetByLabel("Required EUR amount"), "1234.56 €");
        await FillAndCommitAsync(page.GetByLabel("Required percentage"), "12.5%");

        TestReporter.Step(output, "select enum values");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (sentinel)"), "SaaS");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (nullable)"), "Finance");

        TestReporter.Step(output, "select required choices");
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-single-choice-opt-'][data-option-value='Alpha']")
        );
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-multi-choice-opt-'][data-option-value='Alpha']")
        );

        TestReporter.Step(output, "submit");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate form" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("All validation rules passed.", new() { Timeout = 15000 });
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
            "el => { el.dispatchEvent(new Event('input', { bubbles: true })); el.dispatchEvent(new Event('change', { bubbles: true })); }"
        );
    }

    private static async Task SelectByLabelAndCommitAsync(ILocator locator, string label)
    {
        await locator.SelectOptionAsync(new[] { new SelectOptionValue { Label = label } });
        await locator.EvaluateAsync(
            "el => el.dispatchEvent(new Event('change', { bubbles: true }))"
        );
    }

    private static async Task CheckAndCommitAsync(ILocator locator)
    {
        await locator.CheckAsync();
        await locator.EvaluateAsync(
            "el => el.dispatchEvent(new Event('change', { bubbles: true }))"
        );
    }
}

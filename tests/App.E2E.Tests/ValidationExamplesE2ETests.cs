using Microsoft.Playwright;
using Assertions = Microsoft.Playwright.Assertions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class ValidationExamplesE2ETests
{
    private readonly AppHostFixture _host;
    private readonly PlaywrightFixture _playwright;
    private readonly ITestOutputHelper _output;

    public ValidationExamplesE2ETests(
        AppHostFixture host,
        PlaywrightFixture playwright,
        ITestOutputHelper output
    )
    {
        this._host = host;
        this._playwright = playwright;
        this._output = output;
    }

    [Fact]
    public async Task Validation_examples_submits_when_required_fields_are_valid()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/validation-examples");
        await NavigateAndWaitForWasmAsync(page, "/validation-examples");

        await Assertions
            .Expect(
                page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Validation Examples" }
                )
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "fill required text inputs");
        await FillAndCommitAsync(page.GetByLabel("Required SSN"), "010199-8148");
        await FillAndCommitAsync(page.GetByLabel("Required business ID"), "2617416-4");
        await FillAndCommitAsync(page.GetByLabel("Required IBAN"), "NL91 ABNA 0417 1643 00");
        await FillAndCommitAsync(page.GetByLabel("Required email"), "name@example.com");
        await FillAndCommitAsync(page.GetByLabel("Required decimal (fi-FI)"), "1234.56");
        await FillAndCommitAsync(page.GetByLabel("Required EUR amount"), "1234.56 €");
        await FillAndCommitAsync(page.GetByLabel("Required percentage"), "12.5%");

        TestReporter.Step(_output, "select enum values");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (sentinel)"), "SaaS");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (nullable)"), "Finance");

        TestReporter.Step(_output, "select required choices");
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-single-choice-opt-'][data-option-value='Alpha']")
        );
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-multi-choice-opt-'][data-option-value='Alpha']")
        );

        TestReporter.Step(_output, "submit");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate form" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("All validation rules passed.", new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Validation_examples_other_requires_text_then_succeeds_when_fixed()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/validation-examples");
        await NavigateAndWaitForWasmAsync(page, "/validation-examples");

        await Assertions
            .Expect(
                page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "Validation Examples" }
                )
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "fill required text inputs");
        await FillAndCommitAsync(page.GetByLabel("Required SSN"), "010199-8148");
        await FillAndCommitAsync(page.GetByLabel("Required business ID"), "2617416-4");
        await FillAndCommitAsync(page.GetByLabel("Required IBAN"), "NL91 ABNA 0417 1643 00");
        await FillAndCommitAsync(page.GetByLabel("Required email"), "name@example.com");
        await FillAndCommitAsync(page.GetByLabel("Required decimal (fi-FI)"), "1234.56");
        await FillAndCommitAsync(page.GetByLabel("Required EUR amount"), "1234.56 €");
        await FillAndCommitAsync(page.GetByLabel("Required percentage"), "12.5%");

        TestReporter.Step(_output, "select enum values");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (sentinel)"), "SaaS");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry (nullable)"), "Finance");

        TestReporter.Step(_output, "select Other without providing text");
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-single-choice-opt-'][data-option-value='Other']")
        );
        await CheckAndCommitAsync(
            page.Locator("input[id^='required-multi-choice-opt-'][data-option-value='Other']")
        );

        TestReporter.Step(_output, "submit with missing Other text");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate form" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });
        await Assertions
            .Expect(
                page.GetByText("Muu-vaihtoehto vaatii lisäarvon.", new() { Exact = true }).First
            )
            .ToBeVisibleAsync(new() { Timeout = 15000 });

        TestReporter.Step(_output, "fill Other values and submit again");
        await FillAndCommitAsync(page.Locator("input#required-single-choice-other-value"), "Delta");
        await FillAndCommitAsync(
            page.Locator("input#required-multi-choice-other-value"),
            "Epsilon"
        );

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Validate form" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("All validation rules passed.", new() { Timeout = 15000 });
        await Assertions
            .Expect(page.GetByText("Muu-vaihtoehto vaatii lisäarvon.", new() { Exact = true }))
            .ToHaveCountAsync(0, new() { Timeout = 15000 });
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

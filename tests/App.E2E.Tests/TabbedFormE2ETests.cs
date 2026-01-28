using System.Globalization;
using Microsoft.Playwright;
using Assertions = Microsoft.Playwright.Assertions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class TabbedFormE2ETests
{
    private readonly AppHostFixture host;
    private readonly PlaywrightFixture playwright;
    private readonly ITestOutputHelper output;

    public TabbedFormE2ETests(
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
    public async Task Tabbed_form_submits_with_valid_data()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/tabbed-form");
        await NavigateAndWaitForWasmAsync(page, "/tabbed-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tabbed Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "fill customer details");
        await FillAndCommitAsync(page.GetByLabel("Customer name"), "Acme Corp");
        await FillAndCommitAsync(page.GetByLabel("Contact email"), "hello@acme.com");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry"), "SaaS");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();

        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        TestReporter.Step(output, "fill deal details");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Contract type"), "New");
        await FillAndCommitAsync(page.GetByLabel("Seats"), "25");
        await FillAndCommitAsync(page.GetByLabel("Estimated annual value"), "120000");

        var startDate = DateTime
            .Today.AddDays(7)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await FillAndCommitAsync(page.GetByLabel("Expected start date"), startDate);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();

        await Assertions.Expect(page.GetByLabel("Notes")).ToBeVisibleAsync();

        TestReporter.Step(output, "fill notes and submit");
        await FillAndCommitAsync(page.GetByLabel("Notes"), "E2E run notes.");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Submission ready for CRM.", new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Tabbed_form_submit_empty_shows_validation_errors()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/tabbed-form");
        await NavigateAndWaitForWasmAsync(page, "/tabbed-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tabbed Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "go to final tab without filling");
        await GoToFinalTabAsync(page);

        TestReporter.Step(output, "submit empty form");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });

        var summaryText = await page.Locator(".border-rose-200").InnerTextAsync();
        Assert.Contains("Asiakkaan nimi on pakollinen.", summaryText, StringComparison.Ordinal);
        Assert.Contains("Yhteyssähköposti on pakollinen.", summaryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Tabbed_form_errors_persist_when_navigating_tabs_after_failed_submit()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/tabbed-form");
        await NavigateAndWaitForWasmAsync(page, "/tabbed-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tabbed Form" })
            )
            .ToBeVisibleAsync();

        await GoToFinalTabAsync(page);

        TestReporter.Step(output, "submit empty form to generate errors");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });

        TestReporter.Step(output, "navigate back across tabs");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Customer name")).ToBeVisibleAsync();

        var summaryText = await page.Locator(".border-rose-200").InnerTextAsync();
        Assert.Contains("Asiakkaan nimi on pakollinen.", summaryText, StringComparison.Ordinal);
        Assert.Contains("Yhteyssähköposti on pakollinen.", summaryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Tabbed_form_invalid_then_fix_and_submit_succeeds()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/tabbed-form");
        await NavigateAndWaitForWasmAsync(page, "/tabbed-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tabbed Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "go to final tab and submit invalid form");
        await GoToFinalTabAsync(page);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });

        TestReporter.Step(output, "navigate back and fix customer details");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Customer name")).ToBeVisibleAsync();

        await FillAndCommitAsync(page.GetByLabel("Customer name"), "Acme Corp");
        await FillAndCommitAsync(page.GetByLabel("Contact email"), "hello@acme.com");
        await SelectByLabelAndCommitAsync(page.GetByLabel("Industry"), "SaaS");

        TestReporter.Step(output, "fix deal details and submit");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        await SelectByLabelAndCommitAsync(page.GetByLabel("Contract type"), "New");
        await FillAndCommitAsync(page.GetByLabel("Seats"), "25");
        await FillAndCommitAsync(page.GetByLabel("Estimated annual value"), "120000");

        var startDate = DateTime
            .Today.AddDays(7)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await FillAndCommitAsync(page.GetByLabel("Expected start date"), startDate);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Notes")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Submission ready for CRM.", new() { Timeout = 15000 });
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

    private static async Task SelectByLabelAndCommitAsync(ILocator locator, string label)
    {
        await locator.SelectOptionAsync(new[] { new SelectOptionValue { Label = label } });
        await locator.EvaluateAsync(
            "el => el.dispatchEvent(new Event('change', { bubbles: true }))"
        );
    }

    private static async Task GoToFinalTabAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();
        await Assertions.Expect(page.GetByLabel("Notes")).ToBeVisibleAsync();
    }
}

using Microsoft.Playwright;
using Assertions = Microsoft.Playwright.Assertions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class SampleFormE2ETests
{
    private readonly AppHostFixture _host;
    private readonly PlaywrightFixture _playwright;
    private readonly ITestOutputHelper _output;

    public SampleFormE2ETests(
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
    public async Task Sample_form_shows_local_validation_errors()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/sample-form");
        await NavigateAndWaitForWasmAsync(page, "/sample-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "submit with empty values");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });

        await Assertions
            .Expect(page.Locator(".text-rose-600"))
            .ToHaveCountAsync(2, new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Sample_form_accepts_valid_submission()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/sample-form");
        await NavigateAndWaitForWasmAsync(page, "/sample-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "fill name and age");
        await FillAndCommitAsync(page.GetByLabel("Name"), "Jane");
        await FillAndCommitAsync(page.GetByLabel("Age"), "30");

        TestReporter.Step(_output, "submit valid form");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Form is valid.", new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Sample_form_invalid_then_fix_and_submit_succeeds()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/sample-form");
        await NavigateAndWaitForWasmAsync(page, "/sample-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "submit invalid form first");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });

        await Assertions
            .Expect(page.GetByText("Nimi on pakollinen.", new() { Exact = true }).First)
            .ToBeVisibleAsync(new() { Timeout = 15000 });
        await Assertions
            .Expect(page.GetByText("Iän tulee olla välillä 18–120.", new() { Exact = true }).First)
            .ToBeVisibleAsync(new() { Timeout = 15000 });

        TestReporter.Step(_output, "fix inputs and submit again");
        await FillAndCommitAsync(page.GetByLabel("Name"), "Jane");
        await FillAndCommitAsync(page.GetByLabel("Age"), "30");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Form is valid.", new() { Timeout = 15000 });

        await Assertions
            .Expect(page.GetByText("Nimi on pakollinen.", new() { Exact = true }))
            .ToHaveCountAsync(0, new() { Timeout = 15000 });
        await Assertions
            .Expect(page.GetByText("Iän tulee olla välillä 18–120.", new() { Exact = true }))
            .ToHaveCountAsync(0, new() { Timeout = 15000 });
    }

    [Fact]
    public async Task Sample_form_server_error_then_fix_and_submit_succeeds()
    {
        await using var context = await _playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = _host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(_output, $"navigate {_host.BaseUrl}/sample-form");
        await NavigateAndWaitForWasmAsync(page, "/sample-form");

        await Assertions
            .Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(_output, "trigger server-side validation error");
        await FillAndCommitAsync(page.GetByLabel("Name"), "Server");
        await FillAndCommitAsync(page.GetByLabel("Age"), "30");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.", new() { Timeout = 15000 });
        await Assertions
            .Expect(page.GetByText("Nimi ei voi olla 'Server'.", new() { Exact = true }).First)
            .ToBeVisibleAsync(new() { Timeout = 15000 });

        TestReporter.Step(_output, "fix server error and submit again");
        await FillAndCommitAsync(page.GetByLabel("Name"), "Jane");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions
            .Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Form is valid.", new() { Timeout = 15000 });
        await Assertions
            .Expect(page.GetByText("Nimi ei voi olla 'Server'.", new() { Exact = true }))
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
            "el => el.dispatchEvent(new Event('change', { bubbles: true }))"
        );
    }
}

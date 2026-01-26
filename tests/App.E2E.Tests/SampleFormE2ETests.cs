using Microsoft.Playwright;
using Microsoft.Playwright.Assertions;
using Xunit.Abstractions;

namespace App.E2E.Tests;

[Collection("E2E")]
public sealed class SampleFormE2ETests
{
    private readonly AppHostFixture host;
    private readonly PlaywrightFixture playwright;
    private readonly ITestOutputHelper output;

    public SampleFormE2ETests(
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
    public async Task Sample_form_shows_local_validation_errors()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/sample-form");
        await page.GotoAsync("/sample-form");

        await Assertions.Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "submit with empty values");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions.Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Please fix the validation errors.");

        await Assertions.Expect(page.Locator(".text-rose-600")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task Sample_form_accepts_valid_submission()
    {
        await using var context = await playwright.Browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = host.BaseUrl }
        );
        var page = await context.NewPageAsync();

        TestReporter.Step(output, $"navigate {host.BaseUrl}/sample-form");
        await page.GotoAsync("/sample-form");

        await Assertions.Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sample Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "fill name and age");
        await page.GetByLabel("Name").FillAsync("Jane");
        await page.GetByLabel("Age").FillAsync("30");

        TestReporter.Step(output, "submit valid form");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions.Expect(page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Form is valid.");
    }
}

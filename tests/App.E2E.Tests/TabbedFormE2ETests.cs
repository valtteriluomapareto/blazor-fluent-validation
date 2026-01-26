using System.Globalization;
using Microsoft.Playwright;
using Microsoft.Playwright.Assertions;
using Xunit.Abstractions;

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
        await page.GotoAsync("/tabbed-form");

        await Assertions.Expect(
                page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tabbed Form" })
            )
            .ToBeVisibleAsync();

        TestReporter.Step(output, "fill customer details");
        await page.GetByLabel("Customer name").FillAsync("Acme Corp");
        await page.GetByLabel("Contact email").FillAsync("hello@acme.com");
        await page.GetByLabel("Industry").SelectOptionAsync("SaaS");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();

        await Assertions.Expect(page.GetByLabel("Contract type")).ToBeVisibleAsync();

        TestReporter.Step(output, "fill deal details");
        await page.GetByLabel("Contract type").SelectOptionAsync("New");
        await page.GetByLabel("Seats").FillAsync("25");
        await page.GetByLabel("Estimated annual value").FillAsync("120000");

        var startDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await page.GetByLabel("Expected start date").FillAsync(startDate);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next" })
            .ClickAsync();

        await Assertions.Expect(page.GetByLabel("Notes")).ToBeVisibleAsync();

        TestReporter.Step(output, "fill notes and submit");
        await page.GetByLabel("Notes").FillAsync("E2E run notes.");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Assertions.Expect(page.GetByRole(AriaRole.Status))
            .ToHaveTextAsync("Submission ready for CRM.");
    }
}

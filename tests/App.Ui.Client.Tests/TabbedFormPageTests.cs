using System;
using System.Linq;
using AngleSharp.Dom;
using App.Contracts;
using App.Validation;
using Bunit;
using FluentValidation;
using FormValidationTest.Client.Pages;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class TabbedFormPageTests : IDisposable
{
    private readonly BunitContext context = new();

    public TabbedFormPageTests()
    {
        context.Services.AddSingleton<
            IValidator<CustomerIntakeForm>,
            CustomerIntakeFormValidator
        >();
        context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public void TabbedForm_initial_render_shows_customer_tab_fields()
    {
        var cut = context.Render<TabbedForm>();

        Assert.NotNull(cut.Find("input#tab-customer-name"));
        Assert.NotNull(cut.Find("input#tab-contact-email"));
        Assert.NotNull(cut.Find("select#tab-industry"));

        Assert.Empty(cut.FindAll("input#tab-seats"));
        Assert.Empty(cut.FindAll("textarea#tab-notes"));

        var backButton = FindButton(cut, "Back");
        Assert.True(backButton.HasAttribute("disabled"));

        Assert.NotNull(FindButton(cut, "Next"));
    }

    [Fact]
    public void TabbedForm_can_navigate_between_tabs()
    {
        var cut = context.Render<TabbedForm>();

        FindButton(cut, "Next").Click();

        Assert.NotNull(cut.Find("select#tab-contract-type"));
        Assert.NotNull(cut.Find("input#tab-seats"));
        Assert.Empty(cut.FindAll("input#tab-customer-name"));

        FindButton(cut, "Back").Click();

        Assert.NotNull(cut.Find("input#tab-customer-name"));
        Assert.Empty(cut.FindAll("input#tab-seats"));
    }

    [Fact]
    public void TabbedForm_invalid_then_fixing_values_allows_submission()
    {
        var cut = context.Render<TabbedForm>();

        // Move to the final tab and submit with invalid defaults.
        FindButton(cut, "Next").Click();
        FindButton(cut, "Next").Click();

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
            Assert.Contains("Please fix the validation errors.", cut.Markup)
        );

        // Fill required values across the earlier tabs.
        FindButton(cut, "Back").Click();
        FindButton(cut, "Back").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("input#tab-customer-name")));

        cut.Find("input#tab-customer-name").Change("Acme Corp");
        cut.Find("input#tab-contact-email").Change("sales@acme.fi");
        SelectFirstNonDefaultOption(cut, "select#tab-industry");

        cut.WaitForAssertion(() =>
            Assert.DoesNotContain("Please fix the validation errors.", cut.Markup)
        );

        FindButton(cut, "Next").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("select#tab-contract-type")));

        SelectFirstNonDefaultOption(cut, "select#tab-contract-type");
        cut.Find("input#tab-seats").Change("25");

        FindButton(cut, "Next").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("textarea#tab-notes")));

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            var status = cut.Find("div[role='status']");
            Assert.Contains("Submission ready for CRM.", status.TextContent);
        });
    }

    private static void SelectFirstNonDefaultOption<TComponent>(
        IRenderedComponent<TComponent> cut,
        string selectCss
    )
        where TComponent : IComponent
    {
        var options = cut.FindAll($"{selectCss} option");
        var option = options.Skip(1).First();
        var value = option.GetAttribute("value") ?? option.TextContent;

        Assert.False(string.IsNullOrWhiteSpace(value));
        cut.Find(selectCss).Change(value);
    }

    private static IElement FindButton<TComponent>(IRenderedComponent<TComponent> cut, string label)
        where TComponent : IComponent =>
        cut.FindAll("button")
            .Single(button =>
                string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal)
            );
}

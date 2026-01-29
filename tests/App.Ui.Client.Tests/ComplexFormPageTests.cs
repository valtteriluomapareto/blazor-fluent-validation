using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Pages;
using FormValidationTest.Client.Services.Forms;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class ComplexFormPageTests : IDisposable
{
    private readonly BunitContext _context = new();

    public ComplexFormPageTests()
    {
        _context.Services.AddSingleton<
            IValidator<CustomerIntakeForm>,
            CustomerIntakeFormValidator
        >();
        _context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
        _context.Services.AddScoped<IApiFormSubmitter, ApiFormSubmitter>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Api:BaseUrl"] = "http://localhost/" }
            )
            .Build();
        _context.Services.AddSingleton<IConfiguration>(configuration);

        var httpClient = new HttpClient(
            new FormSubmitHandler(
                new Dictionary<string, string>
                {
                    ["/api/complex-form"] = "Complex form submitted to the integration.",
                }
            )
        )
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _context.Services.AddSingleton(httpClient);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void ComplexForm_renders_expected_fields()
    {
        var cut = _context.Render<ComplexForm>();

        Assert.NotNull(cut.Find("input#customer-name"));
        Assert.NotNull(cut.Find("input#contact-email"));
        Assert.NotNull(cut.Find("input#social-security-number"));
        Assert.NotNull(cut.Find("input#business-id"));
        Assert.NotNull(cut.Find("input#vat-number"));
        Assert.NotNull(cut.Find("select#industry"));
        Assert.NotNull(cut.Find("select#contract-type"));
        Assert.NotNull(cut.Find("input#seats"));
        Assert.NotNull(cut.Find("input#estimated-value"));
        Assert.NotNull(cut.Find("input#start-date"));
        Assert.NotNull(cut.Find("textarea#notes"));
    }

    [Fact]
    public void ComplexForm_invalid_then_fixing_values_allows_submission()
    {
        var cut = _context.Render<ComplexForm>();

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Please fix the validation errors.", cut.Markup);
        });

        cut.Find("input#customer-name").Change("Acme Corp");
        cut.Find("input#contact-email").Change("sales@acme.fi");
        SelectFirstNonDefaultOption(cut, "select#industry");
        SelectFirstNonDefaultOption(cut, "select#contract-type");
        cut.Find("input#seats").Change("25");

        cut.WaitForAssertion(() =>
            Assert.DoesNotContain("Please fix the validation errors.", cut.Markup)
        );

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            var status = cut.Find("div[role='status']");
            Assert.Contains("Complex form submitted to the integration.", status.TextContent);
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
}

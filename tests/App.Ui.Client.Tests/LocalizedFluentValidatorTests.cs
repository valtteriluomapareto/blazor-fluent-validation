using System.Linq.Expressions;
using App.Abstractions;
using App.Contracts;
using App.Validation;
using Blazilla;
using FluentValidation;
using FormValidationTest.Client.Components.Forms;
using FormValidationTest.Client.Services;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class LocalizedFluentValidatorTests : IDisposable
{
    private readonly BunitContext _context = new();

    public LocalizedFluentValidatorTests()
    {
        _context.Services.AddSingleton<IUsedNameLookup, LocalUsedNameLookup>();
        _context.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
        _context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task LocalizedFluentValidator_respects_ruleset_override_property()
    {
        var model = new SampleForm { Name = "Server", Age = 10 };
        var editContext = new EditContext(model);
        editContext.Properties[LocalizedFluentValidator.RuleSetProperty] = new[] { "Server" };

        RenderForm(editContext, includeRuleSets: null, allRules: false);

        var isValid = await editContext.ValidateAsync();
        Assert.False(isValid);

        var nameField = new FieldIdentifier(model, nameof(SampleForm.Name));
        var ageField = new FieldIdentifier(model, nameof(SampleForm.Age));

        Assert.Contains("Nimi ei voi olla 'Server'.", editContext.GetValidationMessages(nameField));
        Assert.Empty(editContext.GetValidationMessages(ageField));
    }

    [Fact]
    public async Task LocalizedFluentValidator_all_rules_includes_local_and_server()
    {
        var model = new SampleForm { Name = "Server", Age = 10 };
        var editContext = new EditContext(model);

        RenderForm(editContext, includeRuleSets: null, allRules: true);

        var isValid = await editContext.ValidateAsync();
        Assert.False(isValid);

        var nameField = new FieldIdentifier(model, nameof(SampleForm.Name));
        var ageField = new FieldIdentifier(model, nameof(SampleForm.Age));

        Assert.Contains("Nimi ei voi olla 'Server'.", editContext.GetValidationMessages(nameField));
        Assert.Contains(
            "Iän tulee olla välillä 18–120.",
            editContext.GetValidationMessages(ageField)
        );
    }

    [Fact]
    public async Task LocalizedFluentValidator_rulesets_parameter_limits_validation()
    {
        var model = new SampleForm { Name = "Server", Age = 10 };
        var editContext = new EditContext(model);

        RenderForm(editContext, includeRuleSets: new[] { "Local" }, allRules: false);

        var isValid = await editContext.ValidateAsync();
        Assert.False(isValid);

        var nameField = new FieldIdentifier(model, nameof(SampleForm.Name));
        var ageField = new FieldIdentifier(model, nameof(SampleForm.Age));

        Assert.Empty(editContext.GetValidationMessages(nameField));
        Assert.Contains(
            "Iän tulee olla välillä 18–120.",
            editContext.GetValidationMessages(ageField)
        );
    }

    [Fact]
    public async Task LocalizedFluentValidator_ruleset_override_property_supersedes_parameter()
    {
        var model = new SampleForm { Name = "Server", Age = 10 };
        var editContext = new EditContext(model);
        editContext.Properties[LocalizedFluentValidator.RuleSetProperty] = new[] { "Server" };

        _ = RenderForm(editContext, includeRuleSets: new[] { "Local" }, allRules: false);

        var isValid = await editContext.ValidateAsync();
        Assert.False(isValid);

        var nameField = new FieldIdentifier(model, nameof(SampleForm.Name));
        var ageField = new FieldIdentifier(model, nameof(SampleForm.Age));

        Assert.Contains("Nimi ei voi olla 'Server'.", editContext.GetValidationMessages(nameField));
        Assert.Empty(editContext.GetValidationMessages(ageField));
    }

    [Fact]
    public async Task LocalizedFluentValidator_field_change_updates_target_field_only()
    {
        var model = new SampleForm { Name = string.Empty, Age = 10 };
        var editContext = new EditContext(model);
        var nameField = new FieldIdentifier(model, nameof(SampleForm.Name));
        var ageField = new FieldIdentifier(model, nameof(SampleForm.Age));

        var cut = RenderForm(editContext, includeRuleSets: new[] { "Local" }, allRules: false);

        var isValid = await editContext.ValidateAsync();
        Assert.False(isValid);
        Assert.Contains("Nimi on pakollinen.", editContext.GetValidationMessages(nameField));
        Assert.Contains(
            "Iän tulee olla välillä 18–120.",
            editContext.GetValidationMessages(ageField)
        );

        model.Name = "Valid";
        editContext.NotifyFieldChanged(nameField);

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(editContext.GetValidationMessages(nameField));
            Assert.Contains(
                "Iän tulee olla välillä 18–120.",
                editContext.GetValidationMessages(ageField)
            );
        });

        model.Age = 25;
        editContext.NotifyFieldChanged(ageField);

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(editContext.GetValidationMessages(nameField));
            Assert.Empty(editContext.GetValidationMessages(ageField));
        });
    }

    private IRenderedComponent<EditForm> RenderForm(
        EditContext editContext,
        string[]? includeRuleSets,
        bool allRules
    )
    {
        RenderFragment<EditContext> childContent = _ =>
            builder =>
            {
                builder.OpenComponent<LocalizedFluentValidator>(0);
                builder.AddAttribute(1, nameof(LocalizedFluentValidator.AsyncMode), true);
                builder.AddAttribute(2, nameof(LocalizedFluentValidator.AllRules), allRules);

                if (includeRuleSets is not null)
                {
                    builder.AddAttribute(
                        3,
                        nameof(LocalizedFluentValidator.RuleSets),
                        includeRuleSets
                    );
                }

                builder.CloseComponent();

                builder.OpenComponent<FormTextField>(10);
                builder.AddAttribute(11, nameof(FormTextField.Id), "name");
                builder.AddAttribute(12, nameof(FormTextField.Label), "Name");
                builder.AddAttribute(
                    13,
                    nameof(FormTextField.Value),
                    editContext.Model is SampleForm form ? form.Name : string.Empty
                );
                builder.AddAttribute(
                    14,
                    nameof(FormTextField.ValueExpression),
                    (Expression<Func<string>>)(() => ((SampleForm)editContext.Model).Name)
                );
                builder.CloseComponent();

                builder.OpenComponent<FormNumberField>(20);
                builder.AddAttribute(21, nameof(FormNumberField.Id), "age");
                builder.AddAttribute(22, nameof(FormNumberField.Label), "Age");
                builder.AddAttribute(
                    23,
                    nameof(FormNumberField.Value),
                    ((SampleForm)editContext.Model).Age
                );
                builder.AddAttribute(
                    24,
                    nameof(FormNumberField.ValueExpression),
                    (Expression<Func<int>>)(() => ((SampleForm)editContext.Model).Age)
                );
                builder.CloseComponent();
            };

        return _context.Render<EditForm>(parameters =>
            parameters.Add(p => p.EditContext, editContext).Add(p => p.ChildContent, childContent)
        );
    }
}

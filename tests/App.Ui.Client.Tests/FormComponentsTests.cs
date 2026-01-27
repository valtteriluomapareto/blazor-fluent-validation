using System.Linq.Expressions;
using App.Contracts;
using App.Validation;
using Blazilla;
using FluentValidation;
using FormValidationTest.Client.Components.Forms;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class FormComponentsTests : IDisposable
{
    private readonly BunitContext context = new();

    public FormComponentsTests()
    {
        context.Services.AddSingleton<
            IValidator<ValidationExamplesForm>,
            ValidationExamplesFormValidator
        >();
        context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public void FormCard_renders_child_content()
    {
        var cut = context.Render<FormCard>(parameters =>
            parameters.AddChildContent("<p>Card content</p>")
        );

        Assert.Contains("Card content", cut.Markup);
        Assert.Contains("rounded-xl", cut.Markup);
    }

    [Fact]
    public void FormField_renders_label_and_help_text()
    {
        var cut = context.Render<FormField<string>>(parameters =>
            parameters
                .Add(p => p.Id, "field")
                .Add(p => p.Label, "Field label")
                .Add(p => p.HelpText, "Helpful hint")
                .AddChildContent("<input id=\"field\" />")
        );

        Assert.Equal("Field label", cut.Find("label").TextContent);
        Assert.Contains("Helpful hint", cut.Markup);
        Assert.NotNull(cut.Find("input#field"));
    }

    [Fact]
    public void FormTextField_renders_text_input()
    {
        var cut = context.Render<FormTextField>(parameters =>
            parameters.Add(p => p.Id, "name").Add(p => p.Label, "Name").Add(p => p.Value, "Acme")
        );

        var input = cut.Find("input#name");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("Acme", input.GetAttribute("value"));
    }

    [Fact]
    public void FormNumberField_renders_number_input()
    {
        var cut = context.Render<FormNumberField>(parameters =>
            parameters.Add(p => p.Id, "seats").Add(p => p.Label, "Seats").Add(p => p.Value, 12)
        );

        var input = cut.Find("input#seats");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.Equal("12", input.GetAttribute("value"));
    }

    [Fact]
    public void FormDecimalField_renders_decimal_input()
    {
        var cut = context.Render<FormDecimalField>(parameters =>
            parameters.Add(p => p.Id, "value").Add(p => p.Label, "Value").Add(p => p.Value, 42.5m)
        );

        var input = cut.Find("input#value");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.False(string.IsNullOrWhiteSpace(input.GetAttribute("value")));
    }

    [Fact]
    public void FormDateField_renders_date_input()
    {
        var cut = context.Render<FormDateField>(parameters =>
            parameters
                .Add(p => p.Id, "start")
                .Add(p => p.Label, "Start")
                .Add(p => p.Value, new DateOnly(2026, 1, 15))
        );

        var input = cut.Find("input#start");
        Assert.Equal("date", input.GetAttribute("type"));
        Assert.Equal("2026-01-15", input.GetAttribute("value"));
    }

    [Fact]
    public void FormTextAreaField_renders_textarea()
    {
        var cut = context.Render<FormTextAreaField>(parameters =>
            parameters
                .Add(p => p.Id, "notes")
                .Add(p => p.Label, "Notes")
                .Add(p => p.Value, "Some notes")
                .Add(p => p.Rows, 3)
        );

        var textarea = cut.Find("textarea#notes");
        Assert.Equal("3", textarea.GetAttribute("rows"));
    }

    [Fact]
    public void FormSelectEnumField_renders_all_enum_options()
    {
        var cut = context.Render<FormSelectEnumField<TestOption>>(parameters =>
            parameters
                .Add(p => p.Id, "option")
                .Add(p => p.Label, "Option")
                .Add(p => p.Value, TestOption.Unknown)
        );

        var options = cut.FindAll("select#option option");
        Assert.Equal(3, options.Count);
        Assert.Equal("Unknown", options[0].TextContent);
        Assert.Equal("Alpha", options[1].TextContent);
        Assert.Equal("Beta", options[2].TextContent);
    }

    [Fact]
    public void FormSelectEnumField_includes_placeholder_when_requested()
    {
        var cut = context.Render<FormSelectEnumField<TestOption>>(parameters =>
            parameters
                .Add(p => p.Id, "option")
                .Add(p => p.Label, "Option")
                .Add(p => p.Value, TestOption.Unknown)
                .Add(p => p.IncludePlaceholder, true)
                .Add(p => p.PlaceholderLabel, "Select an option")
        );

        var options = cut.FindAll("select#option option");
        Assert.Equal("Select an option", options[0].TextContent);
        Assert.DoesNotContain(
            options,
            option => string.Equals(option.TextContent, "Unknown", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void FormSelectEnumField_requires_defined_placeholder_sentinel()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.Render<FormSelectEnumField<NoZeroOption>>(parameters =>
                parameters
                    .Add(p => p.Id, "no-zero")
                    .Add(p => p.Label, "No zero")
                    .Add(p => p.Value, NoZeroOption.Alpha)
                    .Add(p => p.IncludePlaceholder, true)
            )
        );

        Assert.Contains(
            nameof(FormSelectEnumField<NoZeroOption>),
            exception.Message,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void FormSelectEnumNullableField_allows_null_placeholder_and_selection()
    {
        var model = new NullableEnumModel();

        var cut = RenderNullableSelect();
        var options = cut.FindAll("select#nullable-option option");
        Assert.Equal("Select an option", options[0].TextContent);

        var select = cut.Find("select#nullable-option");
        select.Change(NullableOption.Alpha);

        cut = RenderNullableSelect();
        Assert.Equal(NullableOption.Alpha, model.Option);

        select = cut.Find("select#nullable-option");
        select.Change(string.Empty);

        cut = RenderNullableSelect();
        Assert.Null(model.Option);

        IRenderedComponent<FormSelectEnumNullableField<NullableOption>> RenderNullableSelect()
        {
            return context.Render<FormSelectEnumNullableField<NullableOption>>(parameters =>
                parameters
                    .Add(p => p.Id, "nullable-option")
                    .Add(p => p.Label, "Nullable option")
                    .Add(p => p.Value, model.Option)
                    .Add(
                        p => p.ValueChanged,
                        EventCallback.Factory.Create<NullableOption?>(
                            this,
                            value => model.Option = value
                        )
                    )
                    .Add(p => p.PlaceholderDisabled, false)
            );
        }
    }

    [Fact]
    public void FormTabs_invokes_active_tab_changed()
    {
        var activeTab = 0;

        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "One", "Two", "Three" })
                .Add(p => p.ActiveTab, activeTab)
                .Add(
                    p => p.ActiveTabChanged,
                    EventCallback.Factory.Create<int>(this, index => activeTab = index)
                )
        );

        cut.FindAll("button")[1].Click();

        Assert.Equal(1, activeTab);
    }

    [Fact]
    public void FormRadioGroupField_updates_selected_value()
    {
        var selected = SingleChoiceOption.None;
        var options = new[]
        {
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.None, "None"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Alpha, "Alpha"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Beta, "Beta"),
        };

        var cut = context.Render<FormRadioGroupField<SingleChoiceOption>>(parameters =>
            parameters
                .Add(p => p.Id, "choice")
                .Add(p => p.Label, "Choice")
                .Add(p => p.Value, selected)
                .Add(p => p.Options, options)
                .Add(
                    p => p.ValueChanged,
                    EventCallback.Factory.Create<SingleChoiceOption>(
                        this,
                        value => selected = value
                    )
                )
        );

        var betaInput = cut.Find("input[id^='choice-opt-'][data-option-value='Beta']");
        betaInput.Change(SingleChoiceOption.Beta);

        Assert.Equal(SingleChoiceOption.Beta, selected);
    }

    [Fact]
    public void FormCheckboxGroupField_toggles_selection()
    {
        var selected = new List<MultiChoiceOption>();
        var options = new[]
        {
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Alpha, "Alpha"),
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Beta, "Beta"),
        };

        var cut = context.Render<FormCheckboxGroupField<MultiChoiceOption>>(parameters =>
            parameters
                .Add(p => p.Id, "multi")
                .Add(p => p.Label, "Multi")
                .Add(p => p.Value, selected)
                .Add(p => p.Options, options)
                .Add(
                    p => p.ValueChanged,
                    EventCallback.Factory.Create<List<MultiChoiceOption>>(
                        this,
                        value => selected = value
                    )
                )
        );

        var alphaInput = cut.Find("input[id^='multi-opt-'][data-option-value='Alpha']");
        alphaInput.Change(true);
        Assert.Contains(MultiChoiceOption.Alpha, selected);

        alphaInput = cut.Find("input[id^='multi-opt-'][data-option-value='Alpha']");
        alphaInput.Change(false);
        Assert.DoesNotContain(MultiChoiceOption.Alpha, selected);
    }

    [Fact]
    public void FormRadioGroupField_supports_other_option_with_text_value()
    {
        var selected = SingleChoiceOption.None;
        var otherValue = string.Empty;
        var options = new[]
        {
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.None, "None"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Alpha, "Alpha"),
        };
        var otherOption = new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Other, "Other");

        var cut = RenderRadioWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Empty(cut.FindAll("input#choice-other-value"));

        var otherRadio = cut.Find("input[id^='choice-opt-'][data-option-value='Other']");
        otherRadio.Change(SingleChoiceOption.Other);
        cut = RenderRadioWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        var otherInput = cut.Find("input#choice-other-value");
        otherInput.Input("Delta");
        cut = RenderRadioWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Equal("Delta", otherValue);

        var alphaRadio = cut.Find("input[id^='choice-opt-'][data-option-value='Alpha']");
        alphaRadio.Change(SingleChoiceOption.Alpha);
        cut = RenderRadioWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Equal(string.Empty, otherValue);
        Assert.Empty(cut.FindAll("input#choice-other-value"));

        IRenderedComponent<FormRadioGroupField<SingleChoiceOption>> RenderRadioWithOther(
            IReadOnlyList<ChoiceOption<SingleChoiceOption>> radioOptions,
            ChoiceOption<SingleChoiceOption> radioOtherOption,
            SingleChoiceOption currentValue,
            string currentOtherValue,
            Action<SingleChoiceOption> valueChanged
        )
        {
            return context.Render<FormRadioGroupField<SingleChoiceOption>>(parameters =>
                parameters
                    .Add(p => p.Id, "choice")
                    .Add(p => p.Label, "Choice")
                    .Add(p => p.Value, currentValue)
                    .Add(p => p.Options, radioOptions)
                    .Add(p => p.OtherOption, radioOtherOption)
                    .Add(p => p.OtherValue, currentOtherValue)
                    .Add(
                        p => p.ValueChanged,
                        EventCallback.Factory.Create<SingleChoiceOption>(this, valueChanged)
                    )
                    .Add(
                        p => p.OtherValueChanged,
                        EventCallback.Factory.Create<string?>(
                            this,
                            value => otherValue = value ?? string.Empty
                        )
                    )
            );
        }
    }

    [Fact]
    public void FormCheckboxGroupField_supports_other_option_with_text_value()
    {
        var selected = new List<MultiChoiceOption>();
        var otherValue = string.Empty;
        var options = new[]
        {
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Alpha, "Alpha"),
        };
        var otherOption = new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Other, "Other");

        var cut = RenderCheckboxWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Empty(cut.FindAll("input#multi-other-value"));

        var otherCheckbox = cut.Find("input[id^='multi-opt-'][data-option-value='Other']");
        otherCheckbox.Change(true);
        cut = RenderCheckboxWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        var otherInput = cut.Find("input#multi-other-value");
        otherInput.Input("Delta");
        cut = RenderCheckboxWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Equal("Delta", otherValue);
        Assert.Contains(MultiChoiceOption.Other, selected);

        otherCheckbox = cut.Find("input[id^='multi-opt-'][data-option-value='Other']");
        otherCheckbox.Change(false);
        cut = RenderCheckboxWithOther(
            options,
            otherOption,
            selected,
            otherValue,
            value => selected = value
        );

        Assert.Equal(string.Empty, otherValue);
        Assert.DoesNotContain(MultiChoiceOption.Other, selected);
        Assert.Empty(cut.FindAll("input#multi-other-value"));

        IRenderedComponent<FormCheckboxGroupField<MultiChoiceOption>> RenderCheckboxWithOther(
            IReadOnlyList<ChoiceOption<MultiChoiceOption>> checkboxOptions,
            ChoiceOption<MultiChoiceOption> checkboxOtherOption,
            List<MultiChoiceOption> currentValue,
            string currentOtherValue,
            Action<List<MultiChoiceOption>> valueChanged
        )
        {
            return context.Render<FormCheckboxGroupField<MultiChoiceOption>>(parameters =>
                parameters
                    .Add(p => p.Id, "multi")
                    .Add(p => p.Label, "Multi")
                    .Add(p => p.Value, currentValue)
                    .Add(p => p.Options, checkboxOptions)
                    .Add(p => p.OtherOption, checkboxOtherOption)
                    .Add(p => p.OtherValue, currentOtherValue)
                    .Add(
                        p => p.ValueChanged,
                        EventCallback.Factory.Create<List<MultiChoiceOption>>(this, valueChanged)
                    )
                    .Add(
                        p => p.OtherValueChanged,
                        EventCallback.Factory.Create<string?>(
                            this,
                            value => otherValue = value ?? string.Empty
                        )
                    )
            );
        }
    }

    [Fact]
    public void FormTextField_generates_fallback_id_from_value_expression()
    {
        var model = new FallbackIdModel();
        Expression<Func<string>> valueExpression = () => model.Name;
        RenderFragment<EditContext> childContent = _ =>
            builder =>
            {
                builder.OpenComponent<FormTextField>(0);
                builder.AddAttribute(1, nameof(FormTextField.Label), "Name");
                builder.AddAttribute(2, nameof(FormTextField.Value), model.Name);
                builder.AddAttribute(3, nameof(FormTextField.ValueExpression), valueExpression);
                builder.CloseComponent();
            };

        var cut = context.Render<EditForm>(parameters =>
            parameters.Add(p => p.Model, model).Add(p => p.ChildContent, childContent)
        );

        var input = cut.Find("input");
        var label = cut.Find("label");
        var resolvedId = input.Id;

        Assert.False(string.IsNullOrWhiteSpace(resolvedId));
        Assert.Equal(resolvedId, label.GetAttribute("for"));
        Assert.Contains("name-field-", resolvedId, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormRadioGroupField_notifies_edit_context_and_updates_validation_state()
    {
        var model = new ValidationExamplesForm();
        var editContext = new EditContext(model);
        var radioOptions = new[]
        {
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.None, "None"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Alpha, "Alpha"),
        };
        var otherOption = new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Other, "Other");
        Expression<Func<SingleChoiceOption>> valueExpression = () => model.RequiredSingleChoice;
        Expression<Func<string?>> otherExpression = () => model.RequiredSingleChoiceOther;
        var singleChoiceField = new FieldIdentifier(model, nameof(model.RequiredSingleChoice));
        var otherField = new FieldIdentifier(model, nameof(model.RequiredSingleChoiceOther));

        var cut = RenderRadioHarness();

        await editContext.ValidateAsync();
        Assert.NotEmpty(editContext.GetValidationMessages(singleChoiceField));

        cut.Find("input[data-option-value='Alpha']").Change(SingleChoiceOption.Alpha);
        cut = RenderRadioHarness();
        cut.WaitForAssertion(() =>
            Assert.Empty(editContext.GetValidationMessages(singleChoiceField))
        );

        cut.Find("input[data-option-value='Other']").Change(SingleChoiceOption.Other);
        cut = RenderRadioHarness();

        var otherInput = cut.Find("input#required-single-choice-other-value");
        otherInput.Input("Temp");
        cut = RenderRadioHarness();
        cut.WaitForAssertion(() => Assert.Empty(editContext.GetValidationMessages(otherField)));

        otherInput = cut.Find("input#required-single-choice-other-value");
        otherInput.Input(string.Empty);
        cut = RenderRadioHarness();
        cut.WaitForAssertion(() => Assert.NotEmpty(editContext.GetValidationMessages(otherField)));

        otherInput = cut.Find("input#required-single-choice-other-value");
        otherInput.Input("Delta");
        cut = RenderRadioHarness();
        cut.WaitForAssertion(() => Assert.Empty(editContext.GetValidationMessages(otherField)));

        IRenderedComponent<EditForm> RenderRadioHarness()
        {
            RenderFragment<EditContext> childContent = _ =>
                builder =>
                {
                    builder.OpenComponent<LocalizedFluentValidator>(0);
                    builder.AddAttribute(1, nameof(LocalizedFluentValidator.AsyncMode), true);
                    builder.AddAttribute(2, nameof(LocalizedFluentValidator.RuleSets), new[] { "Local" });
                    builder.CloseComponent();

                    builder.OpenComponent<FormRadioGroupField<SingleChoiceOption>>(10);
                    builder.AddAttribute(
                        11,
                        nameof(FormRadioGroupField<SingleChoiceOption>.Id),
                        "required-single-choice"
                    );
                    builder.AddAttribute(
                        12,
                        nameof(FormRadioGroupField<SingleChoiceOption>.Label),
                        "Required choice"
                    );
                    builder.AddAttribute(
                        13,
                        nameof(FormRadioGroupField<SingleChoiceOption>.Value),
                        model.RequiredSingleChoice
                    );
                    builder.AddAttribute(
                        14,
                        nameof(FormRadioGroupField<SingleChoiceOption>.ValueChanged),
                        EventCallback.Factory.Create<SingleChoiceOption>(
                            this,
                            value => model.RequiredSingleChoice = value
                        )
                    );
                    builder.AddAttribute(
                        15,
                        nameof(FormRadioGroupField<SingleChoiceOption>.ValueExpression),
                        valueExpression
                    );
                    builder.AddAttribute(
                        16,
                        nameof(FormRadioGroupField<SingleChoiceOption>.Options),
                        radioOptions
                    );
                    builder.AddAttribute(
                        17,
                        nameof(FormRadioGroupField<SingleChoiceOption>.OtherOption),
                        otherOption
                    );
                    builder.AddAttribute(
                        18,
                        nameof(FormRadioGroupField<SingleChoiceOption>.OtherValue),
                        model.RequiredSingleChoiceOther
                    );
                    builder.AddAttribute(
                        19,
                        nameof(FormRadioGroupField<SingleChoiceOption>.OtherValueChanged),
                        EventCallback.Factory.Create<string?>(
                            this,
                            value => model.RequiredSingleChoiceOther = value ?? string.Empty
                        )
                    );
                    builder.AddAttribute(
                        20,
                        nameof(FormRadioGroupField<SingleChoiceOption>.OtherValueExpression),
                        otherExpression
                    );
                    builder.CloseComponent();

                    builder.OpenComponent<ValidationMessage<SingleChoiceOption>>(30);
                    builder.AddAttribute(
                        31,
                        nameof(ValidationMessage<SingleChoiceOption>.For),
                        valueExpression
                    );
                    builder.CloseComponent();

                    builder.OpenComponent<ValidationMessage<string?>>(40);
                    builder.AddAttribute(
                        41,
                        nameof(ValidationMessage<string?>.For),
                        otherExpression
                    );
                    builder.CloseComponent();
                };

            return context.Render<EditForm>(parameters =>
                parameters
                    .Add(p => p.EditContext, editContext)
                    .Add(p => p.ChildContent, childContent)
            );
        }
    }

    [Fact]
    public async Task FormCheckboxGroupField_notifies_edit_context_and_updates_validation_state()
    {
        var model = new ValidationExamplesForm();
        var editContext = new EditContext(model);
        var checkboxOptions = new[]
        {
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Alpha, "Alpha"),
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Beta, "Beta"),
        };
        var otherOption = new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Other, "Other");
        Expression<Func<List<MultiChoiceOption>>> valueExpression = () => model.RequiredMultiChoice;
        Expression<Func<string?>> otherExpression = () => model.RequiredMultiChoiceOther;
        var multiChoiceField = new FieldIdentifier(model, nameof(model.RequiredMultiChoice));
        var otherField = new FieldIdentifier(model, nameof(model.RequiredMultiChoiceOther));

        var cut = RenderCheckboxHarness();

        await editContext.ValidateAsync();
        Assert.NotEmpty(editContext.GetValidationMessages(multiChoiceField));

        cut.Find("input[data-option-value='Alpha']").Change(true);
        cut = RenderCheckboxHarness();
        cut.WaitForAssertion(() =>
            Assert.Empty(editContext.GetValidationMessages(multiChoiceField))
        );

        cut.Find("input[data-option-value='Other']").Change(true);
        cut = RenderCheckboxHarness();

        var otherInput = cut.Find("input#required-multi-choice-other-value");
        otherInput.Input("Temp");
        cut = RenderCheckboxHarness();
        cut.WaitForAssertion(() => Assert.Empty(editContext.GetValidationMessages(otherField)));

        otherInput = cut.Find("input#required-multi-choice-other-value");
        otherInput.Input(string.Empty);
        cut = RenderCheckboxHarness();
        cut.WaitForAssertion(() => Assert.NotEmpty(editContext.GetValidationMessages(otherField)));

        cut.Find("input[data-option-value='Other']").Change(false);
        cut = RenderCheckboxHarness();
        cut.WaitForAssertion(() => Assert.Empty(editContext.GetValidationMessages(otherField)));

        IRenderedComponent<EditForm> RenderCheckboxHarness()
        {
            RenderFragment<EditContext> childContent = _ =>
                builder =>
                {
                    builder.OpenComponent<LocalizedFluentValidator>(0);
                    builder.AddAttribute(1, nameof(LocalizedFluentValidator.AsyncMode), true);
                    builder.AddAttribute(2, nameof(LocalizedFluentValidator.RuleSets), new[] { "Local" });
                    builder.CloseComponent();

                    builder.OpenComponent<FormCheckboxGroupField<MultiChoiceOption>>(10);
                    builder.AddAttribute(
                        11,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.Id),
                        "required-multi-choice"
                    );
                    builder.AddAttribute(
                        12,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.Label),
                        "Required multi choice"
                    );
                    builder.AddAttribute(
                        13,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.Value),
                        model.RequiredMultiChoice
                    );
                    builder.AddAttribute(
                        14,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.ValueChanged),
                        EventCallback.Factory.Create<List<MultiChoiceOption>>(
                            this,
                            value => model.RequiredMultiChoice = value
                        )
                    );
                    builder.AddAttribute(
                        15,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.ValueExpression),
                        valueExpression
                    );
                    builder.AddAttribute(
                        16,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.Options),
                        checkboxOptions
                    );
                    builder.AddAttribute(
                        17,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.OtherOption),
                        otherOption
                    );
                    builder.AddAttribute(
                        18,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.OtherValue),
                        model.RequiredMultiChoiceOther
                    );
                    builder.AddAttribute(
                        19,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.OtherValueChanged),
                        EventCallback.Factory.Create<string?>(
                            this,
                            value => model.RequiredMultiChoiceOther = value ?? string.Empty
                        )
                    );
                    builder.AddAttribute(
                        20,
                        nameof(FormCheckboxGroupField<MultiChoiceOption>.OtherValueExpression),
                        otherExpression
                    );
                    builder.CloseComponent();

                    builder.OpenComponent<ValidationMessage<List<MultiChoiceOption>>>(30);
                    builder.AddAttribute(
                        31,
                        nameof(ValidationMessage<List<MultiChoiceOption>>.For),
                        valueExpression
                    );
                    builder.CloseComponent();

                    builder.OpenComponent<ValidationMessage<string?>>(40);
                    builder.AddAttribute(
                        41,
                        nameof(ValidationMessage<string?>.For),
                        otherExpression
                    );
                    builder.CloseComponent();
                };

            return context.Render<EditForm>(parameters =>
                parameters
                    .Add(p => p.EditContext, editContext)
                    .Add(p => p.ChildContent, childContent)
            );
        }
    }

    private enum TestOption
    {
        Unknown = 0,
        Alpha = 1,
        Beta = 2,
    }

    private enum NoZeroOption
    {
        Alpha = 1,
        Beta = 2,
    }

    private enum NullableOption
    {
        Alpha = 1,
        Beta = 2,
    }

    private sealed class FallbackIdModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class NullableEnumModel
    {
        public NullableOption? Option { get; set; }
    }
}

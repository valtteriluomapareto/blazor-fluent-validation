using System.Linq.Expressions;
using FormValidationTest.Client.Components.Forms;

namespace App.Ui.Client.Tests;

public sealed class FormFieldIdResolverTests
{
    [Fact]
    public void Resolve_returns_provided_id_when_present()
    {
        var result = FormFieldIdResolver.Resolve<int>("explicit-id", null, componentKey: "abc123");

        Assert.Equal("explicit-id", result);
    }

    [Fact]
    public void Resolve_uses_expression_field_name_and_suffix()
    {
        var model = new TestModel { SomeValue = 42 };
        Expression<Func<int>> expression = () => model.SomeValue;

        var result = FormFieldIdResolver.Resolve(null, expression, componentKey: "abc123");

        Assert.Equal("somevalue-field-abc123", result);
    }

    [Fact]
    public void Resolve_sanitizes_field_name_with_underscores()
    {
        var model = new TestModel { SomeValue = 5 };
        Expression<Func<int>> expression = () => model.Some_Value;

        var result = FormFieldIdResolver.Resolve(null, expression, componentKey: "abc123");

        Assert.Equal("some_value-field-abc123", result);
    }

    [Fact]
    public void Resolve_uses_default_suffix_when_suffix_is_blank()
    {
        var model = new TestModel { SomeValue = 42 };
        Expression<Func<int>> expression = () => model.SomeValue;

        var result = FormFieldIdResolver.Resolve(
            null,
            expression,
            componentKey: "abc123",
            suffix: "   "
        );

        Assert.Equal("somevalue-field-abc123", result);
    }

    [Fact]
    public void Resolve_uses_field_fallback_when_expression_is_missing()
    {
        var result = FormFieldIdResolver.Resolve<int>(null, null, componentKey: "abc123");

        Assert.Equal("field-abc123", result);
    }

    [Fact]
    public void Resolve_sanitizes_suffix_and_prefixes_non_letter()
    {
        var model = new TestModel { SomeValue = 42 };
        Expression<Func<int>> expression = () => model.SomeValue;

        var result = FormFieldIdResolver.Resolve(
            null,
            expression,
            componentKey: "abc123",
            suffix: "123"
        );

        Assert.Equal("somevalue-f-123-abc123", result);
    }

    private sealed class TestModel
    {
        public int SomeValue { get; init; }
        public int Some_Value { get; init; }
    }
}

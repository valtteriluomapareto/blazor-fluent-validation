using App.Contracts;
using App.Validation;
using FluentValidation.TestHelper;

namespace App.Validation.Tests;

public sealed class SampleFormValidatorTests
{
    private readonly SampleFormValidator validator = new();

    [Fact]
    public void Empty_name_should_have_error()
    {
        var model = new SampleForm { Name = "", Age = 25 };

        var result = validator.TestValidate(model, options => options.IncludeRuleSets("Local"));

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.required");
    }

    [Fact]
    public void Age_out_of_range_should_have_error()
    {
        var model = new SampleForm { Name = "Jane", Age = 10 };

        var result = validator.TestValidate(model, options => options.IncludeRuleSets("Local"));

        result.ShouldHaveValidationErrorFor(x => x.Age).WithErrorCode("age.range");
    }

    [Fact]
    public void Valid_input_should_pass()
    {
        var model = new SampleForm { Name = "Jane", Age = 30 };

        var result = validator.TestValidate(model, options => options.IncludeRuleSets("Local"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Server_rule_should_block_reserved_name()
    {
        var model = new SampleForm { Name = "Server", Age = 30 };

        var result = validator.TestValidate(model, options => options.IncludeRuleSets("Server"));

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.server_reserved");
    }
}

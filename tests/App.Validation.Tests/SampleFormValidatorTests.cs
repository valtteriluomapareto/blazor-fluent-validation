using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation.TestHelper;

namespace App.Validation.Tests;

public sealed class SampleFormValidatorTests
{
    private static SampleFormValidator CreateValidator(params string[] usedNames)
    {
        return new SampleFormValidator(new TestUsedNameLookup(usedNames));
    }

    [Fact]
    public async Task Empty_name_should_have_error()
    {
        var model = new SampleForm { Name = "", Age = 25 };

        var result = await CreateValidator().TestValidateAsync(
            model,
            options => options.IncludeRuleSets("Local")
        );

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.required");
    }

    [Fact]
    public async Task Age_out_of_range_should_have_error()
    {
        var model = new SampleForm { Name = "Jane", Age = 10 };

        var result = await CreateValidator().TestValidateAsync(
            model,
            options => options.IncludeRuleSets("Local")
        );

        result.ShouldHaveValidationErrorFor(x => x.Age).WithErrorCode("age.range");
    }

    [Fact]
    public async Task Valid_input_should_pass()
    {
        var model = new SampleForm { Name = "Jane", Age = 30 };

        var result = await CreateValidator().TestValidateAsync(
            model,
            options => options.IncludeRuleSets("Local")
        );

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Server_rule_should_block_reserved_name()
    {
        var model = new SampleForm { Name = "Server", Age = 30 };

        var result = await CreateValidator("Taken").TestValidateAsync(
            model,
            options => options.IncludeRuleSets("Server")
        );

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.server_reserved");
    }

    [Fact]
    public async Task Used_name_should_have_error()
    {
        var model = new SampleForm { Name = "Taken", Age = 30 };

        var result = await CreateValidator("Taken").TestValidateAsync(
            model,
            options => options.IncludeRuleSets("Server")
        );

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.already_used");
    }

    private sealed class TestUsedNameLookup : IUsedNameLookup
    {
        private readonly IReadOnlyCollection<string> usedNames;

        public TestUsedNameLookup(IEnumerable<string> usedNames)
        {
            this.usedNames = usedNames.ToArray();
        }

        public Task<IReadOnlyCollection<string>> GetUsedNamesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(usedNames);
        }
    }
}

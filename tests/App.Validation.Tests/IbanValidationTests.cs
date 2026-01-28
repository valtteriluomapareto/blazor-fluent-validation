namespace App.Validation.Tests;

public sealed class IbanValidationTests
{
    [Fact]
    public void IsValid_should_fail_for_empty_string()
    {
        Assert.False(IbanValidation.IsValid(string.Empty));
    }

    [Fact]
    public void IsValid_should_pass_for_known_valid_iban()
    {
        Assert.True(IbanValidation.IsValid("NL91ABNA0417164300"));
    }

    [Fact]
    public void IsValid_should_pass_for_valid_iban_with_whitespace()
    {
        Assert.True(IbanValidation.IsValid("NL91 ABNA 0417 1643 00"));
    }

    [Fact]
    public void IsValid_should_fail_for_invalid_checksum()
    {
        Assert.False(IbanValidation.IsValid("NL91ABNA0417164301"));
    }
}

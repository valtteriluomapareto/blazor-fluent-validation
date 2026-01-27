using App.Validation;

namespace App.Validation.Tests;

public sealed class FinnishSsnTests
{
    [Fact]
    public void Validate_should_fail_for_empty_string()
    {
        Assert.False(FinnishSsn.Validate(string.Empty));
    }

    [Fact]
    public void Validate_should_fail_for_month_out_of_bounds()
    {
        Assert.False(FinnishSsn.Validate("301398-1233"));
    }

    [Fact]
    public void Validate_should_fail_for_day_out_of_bounds_in_january()
    {
        Assert.False(FinnishSsn.Validate("320198-123P"));
    }

    [Fact]
    public void Validate_should_fail_for_february_29_on_non_leap_year()
    {
        Assert.False(FinnishSsn.Validate("290299-123U"));
    }

    [Fact]
    public void Validate_should_fail_for_day_out_of_bounds_in_february_on_leap_year()
    {
        Assert.False(FinnishSsn.Validate("300204-123Y"));
    }

    [Fact]
    public void Validate_should_fail_for_alphabets_in_birth_date()
    {
        Assert.False(FinnishSsn.Validate("0101AA-123A"));
    }

    [Fact]
    public void Validate_should_fail_for_invalid_separator_for_1900s()
    {
        var invalidSeparatorChars = "ABCDEFGHIJKLMNOPQRST1234567890".ToCharArray();

        foreach (var invalidChar in invalidSeparatorChars)
        {
            Assert.False(FinnishSsn.Validate($"010195{invalidChar}433X"));
            Assert.False(FinnishSsn.Validate($"010195{char.ToLowerInvariant(invalidChar)}433X"));
        }
    }

    [Fact]
    public void Validate_should_fail_for_invalid_separator_for_2000s()
    {
        var invalidSeparatorChars = "GHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        foreach (var invalidChar in invalidSeparatorChars)
        {
            Assert.False(FinnishSsn.Validate($"010103{invalidChar}433X"));
            Assert.False(FinnishSsn.Validate($"010103{char.ToLowerInvariant(invalidChar)}433X"));
        }
    }

    [Fact]
    public void Validate_should_fail_for_too_long_date()
    {
        Assert.False(FinnishSsn.Validate("01011995+433X"));
    }

    [Fact]
    public void Validate_should_fail_for_too_short_date()
    {
        Assert.False(FinnishSsn.Validate("01015+433X"));
    }

    [Fact]
    public void Validate_should_fail_for_too_long_rolling_id()
    {
        Assert.False(FinnishSsn.Validate("010195+4433X"));
    }

    [Fact]
    public void Validate_should_fail_for_too_short_rolling_id()
    {
        Assert.False(FinnishSsn.Validate("010195+33X"));
    }

    [Fact]
    public void Validate_should_pass_for_valid_1800s_code()
    {
        Assert.True(FinnishSsn.Validate("010195+433X"));
    }

    [Fact]
    public void Validate_should_pass_for_valid_1900s_code()
    {
        Assert.True(FinnishSsn.Validate("010197-100P"));
    }

    [Fact]
    public void Validate_should_pass_for_valid_2000s_code()
    {
        Assert.True(FinnishSsn.Validate("010114A173M"));
    }

    [Fact]
    public void Validate_should_pass_for_leap_year_divisible_by_4_only()
    {
        Assert.True(FinnishSsn.Validate("290296-7808"));
    }

    [Fact]
    public void Validate_should_fail_for_year_divisible_by_100_but_not_400()
    {
        Assert.False(FinnishSsn.Validate("290200-101P"));
    }

    [Fact]
    public void Validate_should_fail_for_trailing_whitespace()
    {
        Assert.False(FinnishSsn.Validate("010114A173M "));
    }

    [Fact]
    public void Validate_should_fail_for_leading_whitespace()
    {
        Assert.False(FinnishSsn.Validate(" 010114A173M"));
    }

    [Fact]
    public void Validate_should_pass_for_year_divisible_by_400()
    {
        Assert.True(FinnishSsn.Validate("290200A248A"));
    }

    [Fact]
    public void Validate_should_pass_for_new_intermediate_characters()
    {
        var newHypotheticalIndividuals = new[]
        {
            "010594Y9021",
            "020594X903P",
            "020594X902N",
            "030594W903B",
            "030694W9024",
            "040594V9030",
            "040594V902Y",
            "050594U903M",
            "050594U902L",
            "010516B903X",
            "010516B902W",
            "020516C903K",
            "020516C902J",
            "030516D9037",
            "030516D9026",
            "010501E9032",
            "020502E902X",
            "020503F9037",
            "020504A902E",
            "020504B904H",
            "010594Y9032",
        };

        foreach (var individual in newHypotheticalIndividuals)
        {
            Assert.True(FinnishSsn.Validate(individual));
        }
    }

    [Fact]
    public void Parse_should_parse_valid_male_born_on_leap_day_2000()
    {
        var parsed = FinnishSsn.Parse("290200A717E", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Male, parsed.Sex);
        Assert.Equal(2000, parsed.DateOfBirth.Year);
        Assert.Equal(2, parsed.DateOfBirth.Month);
        Assert.Equal(29, parsed.DateOfBirth.Day);
        Assert.Equal(14, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_parse_valid_female_born_on_1999_01_01()
    {
        var parsed = FinnishSsn.Parse("010199-8148", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(1999, 1, 1), parsed.DateOfBirth);
        Assert.Equal(16, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_parse_valid_female_born_on_2010_12_31()
    {
        var parsed = FinnishSsn.Parse("311210A540N", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(2010, 12, 31), parsed.DateOfBirth);
        Assert.Equal(4, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_parse_valid_male_born_on_1888_02_02()
    {
        var parsed = FinnishSsn.Parse("020288+9818", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Male, parsed.Sex);
        Assert.Equal(new DateOnly(1888, 2, 2), parsed.DateOfBirth);
        Assert.Equal(127, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_parse_zero_years_old_on_day_after_birth()
    {
        var parsed = FinnishSsn.Parse("311215A000J", new DateOnly(2016, 1, 1));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(2015, 12, 31), parsed.DateOfBirth);
        Assert.Equal(0, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_calculate_age_when_birthdate_is_before_today()
    {
        var parsed = FinnishSsn.Parse("130195-1212", new DateOnly(2017, 1, 13));

        Assert.Equal(22, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_calculate_age_when_birthdate_is_after_today()
    {
        var parsed = FinnishSsn.Parse("150295-1212", new DateOnly(2017, 1, 13));

        Assert.Equal(21, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_reject_lowercase_checksum()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FinnishSsn.Parse("311210A540n", new DateOnly(2015, 2, 2))
        );

        Assert.Contains("Not valid SSN format", exception.Message);
    }

    [Fact]
    public void Parse_should_return_invalid_for_wrong_checksum()
    {
        var parsed = FinnishSsn.Parse("150295-1212", new DateOnly(2015, 12, 12));

        Assert.False(parsed.Valid);
    }

    [Fact]
    public void Parse_should_throw_for_month_out_of_bounds()
    {
        var exception = Assert.Throws<ArgumentException>(() => FinnishSsn.Parse("301398-1233"));
        Assert.Contains("Not valid SSN", exception.Message);
    }

    [Fact]
    public void Parse_should_throw_for_day_out_of_bounds()
    {
        var exception = Assert.Throws<ArgumentException>(() => FinnishSsn.Parse("330198-123X"));
        Assert.Contains("Not valid SSN", exception.Message);
    }

    [Fact]
    public void Parse_should_support_new_identity_codes_for_2000s()
    {
        var parsed = FinnishSsn.Parse("290200E717E", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Male, parsed.Sex);
        Assert.Equal(new DateOnly(2000, 2, 29), parsed.DateOfBirth);
        Assert.Equal(14, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_support_new_identity_codes_for_1900s()
    {
        var parsed = FinnishSsn.Parse("010199Y8148", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(1999, 1, 1), parsed.DateOfBirth);
        Assert.Equal(16, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_support_new_identity_codes_for_2010s()
    {
        var parsed = FinnishSsn.Parse("311210F540N", new DateOnly(2015, 2, 2));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(2010, 12, 31), parsed.DateOfBirth);
        Assert.Equal(4, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_support_new_identity_codes_for_recent_births()
    {
        var parsed = FinnishSsn.Parse("311215F000J", new DateOnly(2016, 1, 1));

        Assert.True(parsed.Valid);
        Assert.Equal(FinnishSsn.Female, parsed.Sex);
        Assert.Equal(new DateOnly(2015, 12, 31), parsed.DateOfBirth);
        Assert.Equal(0, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_calculate_age_with_new_identity_code_before_today()
    {
        var parsed = FinnishSsn.Parse("130195Y1212", new DateOnly(2017, 1, 13));

        Assert.Equal(22, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_calculate_age_with_new_identity_code_after_today()
    {
        var parsed = FinnishSsn.Parse("150295V1212", new DateOnly(2017, 1, 13));

        Assert.Equal(21, parsed.AgeInYears);
    }

    [Fact]
    public void Parse_should_reject_lowercase_checksum_with_new_identity_code()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            FinnishSsn.Parse("311210E540n", new DateOnly(2015, 2, 2))
        );

        Assert.Contains("Not valid SSN format", exception.Message);
    }

    [Fact]
    public void Parse_should_return_invalid_for_wrong_checksum_with_new_identity_code()
    {
        var parsed = FinnishSsn.Parse("150295U1212", new DateOnly(2015, 12, 12));

        Assert.False(parsed.Valid);
    }

    [Fact]
    public void Parse_should_throw_for_month_out_of_bounds_with_new_identity_code()
    {
        var exception = Assert.Throws<ArgumentException>(() => FinnishSsn.Parse("301398W1233"));
        Assert.Contains("Not valid SSN", exception.Message);
    }

    [Fact]
    public void Parse_should_throw_for_day_out_of_bounds_with_new_identity_code()
    {
        var exception = Assert.Throws<ArgumentException>(() => FinnishSsn.Parse("330198X123X"));
        Assert.Contains("Not valid SSN", exception.Message);
    }
}

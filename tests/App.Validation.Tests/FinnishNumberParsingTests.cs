using App.Validation;

namespace App.Validation.Tests;

public sealed class FinnishNumberParsingTests
{
    [Fact]
    public void TryParseDecimal_should_accept_finnish_formatted_value()
    {
        var success = FinnishNumberParsing.TryParseDecimal("1 234,56", out var value);

        Assert.True(success);
        Assert.Equal(1234.56m, value);
    }

    [Fact]
    public void TryParseCurrencyEur_should_accept_currency_symbol()
    {
        var success = FinnishNumberParsing.TryParseCurrencyEur("1 234,56 €", out var amount);

        Assert.True(success);
        Assert.Equal(1234.56m, amount);
    }

    [Fact]
    public void TryParsePercentage_should_accept_percent_suffix()
    {
        var success = FinnishNumberParsing.TryParsePercentage("12,5%", out var percentage);

        Assert.True(success);
        Assert.Equal(12.5m, percentage);
    }
}

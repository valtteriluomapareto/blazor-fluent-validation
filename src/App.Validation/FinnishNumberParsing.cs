using System.Globalization;

namespace App.Validation;

public static class FinnishNumberParsing
{
    private static readonly CultureInfo FiCulture = CultureInfo.GetCultureInfo("fi-FI");
    private const NumberStyles Styles = NumberStyles.Number;

    public static bool TryParseDecimal(string? value, out decimal result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = NormalizeWhitespace(value);

        // First try Finnish culture (comma decimals, space thousands).
        if (decimal.TryParse(normalized, Styles, FiCulture, out result))
        {
            return true;
        }

        // Fall back to invariant culture to be more forgiving in demos/tests.
        return decimal.TryParse(normalized, Styles, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseCurrencyEur(string? value, out decimal amount)
    {
        amount = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = NormalizeCurrency(value);
        return TryParseDecimal(normalized, out amount);
    }

    public static bool TryParsePercentage(string? value, out decimal percentage)
    {
        percentage = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = NormalizePercentage(value);
        return TryParseDecimal(normalized, out percentage);
    }

    private static string NormalizeCurrency(string value)
    {
        var withoutSymbol = value
            .Replace("€", string.Empty, StringComparison.Ordinal)
            .Replace("EUR", string.Empty, StringComparison.OrdinalIgnoreCase);

        return NormalizeWhitespace(withoutSymbol);
    }

    private static string NormalizePercentage(string value)
    {
        var withoutSymbol = value.Replace("%", string.Empty, StringComparison.Ordinal);
        return NormalizeWhitespace(withoutSymbol);
    }

    private static string NormalizeWhitespace(string value)
    {
        // Finnish-formatted numbers often use non-breaking spaces as thousand separators.
        var normalized = value.Replace('\u00A0', ' ').Trim();
        return normalized;
    }
}

using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace FormValidationTest.Client.Components.Forms;

internal static class FormFieldIdResolver
{
    public static string Resolve<TValue>(
        string? providedId,
        Expression<Func<TValue>>? valueExpression,
        string componentKey,
        string suffix = "field"
    )
    {
        if (!string.IsNullOrWhiteSpace(providedId))
        {
            return providedId;
        }

        var fieldName = TryGetFieldName(valueExpression);
        var safeFieldName = Sanitize(fieldName);
        var safeSuffix = SanitizeSuffix(suffix);

        if (!string.IsNullOrWhiteSpace(safeFieldName))
        {
            return $"{safeFieldName}-{safeSuffix}-{componentKey}";
        }

        return $"field-{componentKey}";
    }

    private static string? TryGetFieldName<TValue>(Expression<Func<TValue>>? valueExpression)
    {
        if (valueExpression is null)
        {
            return null;
        }

        try
        {
            return FieldIdentifier.Create(valueExpression).FieldName;
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeSuffix(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            return "field";
        }

        var normalized = Convert.ToString(suffix, CultureInfo.InvariantCulture) ?? "field";
        return Sanitize(normalized);
    }

    private static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_')
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else
            {
                builder.Append('-');
            }
        }

        var result = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(result))
        {
            return string.Empty;
        }

        // IDs can start with digits, but prefixing with a letter avoids surprises in CSS selectors.
        if (!char.IsLetter(result[0]))
        {
            return $"f-{result}";
        }

        return result;
    }
}

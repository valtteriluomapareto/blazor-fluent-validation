using IbanNet;

namespace App.Validation;

public static class IbanValidation
{
    private static readonly IIbanValidator Validator = new IbanValidator();

    public static bool IsValid(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var normalized = RemoveWhitespace(iban);
        var result = Validator.Validate(normalized);
        return result.IsValid;
    }

    private static string RemoveWhitespace(string value)
    {
        var buffer = new char[value.Length];
        var index = 0;

        foreach (var c in value)
        {
            if (!char.IsWhiteSpace(c))
            {
                buffer[index++] = c;
            }
        }

        return new string(buffer, 0, index);
    }
}

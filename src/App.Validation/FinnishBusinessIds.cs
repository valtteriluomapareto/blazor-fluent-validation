using System.Text.RegularExpressions;

namespace App.Validation;

public static partial class FinnishBusinessIds
{
    private static readonly int[] Multipliers = [7, 9, 10, 5, 8, 4, 2];

    public static bool IsValidBusinessId(string businessId)
    {
        if (string.IsNullOrWhiteSpace(businessId) || !BusinessIdRegex().IsMatch(businessId))
        {
            return false;
        }

        var givenChecksum = businessId[8] - '0';
        var idNumbers = businessId[..7];
        var calculatedChecksum = CalculateChecksum(idNumbers);

        return calculatedChecksum == givenChecksum;
    }

    public static bool IsValidVatNumber(string vatNumber)
    {
        if (string.IsNullOrWhiteSpace(vatNumber) || !VatNumberRegex().IsMatch(vatNumber))
        {
            return false;
        }

        var vatAsBusinessId = $"{vatNumber.Substring(2, 7)}-{vatNumber[9]}";
        return IsValidBusinessId(vatAsBusinessId);
    }

    public static string GenerateBusinessId()
    {
        var businessId = RandomBusinessIdWithoutChecksum();
        var checksum = CalculateChecksum(businessId);
        return $"{businessId}-{checksum}";
    }

    public static string GenerateVatNumber()
    {
        const string countryCode = "FI";
        var businessId = RandomBusinessIdWithoutChecksum();
        var checksum = CalculateChecksum(businessId);
        return countryCode + businessId + checksum;
    }

    public static int CalculateChecksum(string idNumbers)
    {
        ArgumentNullException.ThrowIfNull(idNumbers);

        var sum = 0;
        for (var i = 0; i < idNumbers.Length && i < Multipliers.Length; i++)
        {
            sum += (idNumbers[i] - '0') * Multipliers[i];
        }

        var remainder = sum % 11;
        if (remainder == 1)
        {
            return -1;
        }

        if (remainder > 1)
        {
            remainder = 11 - remainder;
        }

        return remainder;
    }

    private static string RandomBusinessIdWithoutChecksum()
    {
        while (true)
        {
            var businessId = Random.Shared.Next(1_000_000, 2_000_000).ToString();
            if (CalculateChecksum(businessId) != -1)
            {
                return businessId;
            }
        }
    }

    [GeneratedRegex("^\\d{7}-\\d$", RegexOptions.CultureInvariant)]
    private static partial Regex BusinessIdRegex();

    [GeneratedRegex("^FI\\d{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex VatNumberRegex();
}


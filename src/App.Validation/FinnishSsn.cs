using System.Text.RegularExpressions;

namespace App.Validation;

public static partial class FinnishSsn
{
    public const string Female = "female";
    public const string Male = "male";

    private static readonly IReadOnlyDictionary<char, int> CenturyMap = new Dictionary<char, int>
    {
        ['F'] = 2000,
        ['E'] = 2000,
        ['D'] = 2000,
        ['C'] = 2000,
        ['B'] = 2000,
        ['A'] = 2000,
        ['U'] = 1900,
        ['V'] = 1900,
        ['W'] = 1900,
        ['X'] = 1900,
        ['Y'] = 1900,
        ['-'] = 1900,
        ['+'] = 1800,
    };

    private static readonly int[] DaysInMonth =
    [
        31,
        28,
        31,
        30,
        31,
        30,
        31,
        31,
        30,
        31,
        30,
        31,
    ];

    private static readonly char[] ChecksumTable = "0123456789ABCDEFHJKLMNPRSTUVWXY".ToCharArray();

    public static FinnishSsnParseResult Parse(string ssn, DateOnly? today = null)
    {
        if (string.IsNullOrWhiteSpace(ssn) || !SsnRegex().IsMatch(ssn))
        {
            throw new ArgumentException("Not valid SSN format", nameof(ssn));
        }

        var day = int.Parse(ssn[..2]);
        var month = int.Parse(ssn.Substring(2, 2));
        var centuryId = ssn[6];
        var year = int.Parse(ssn.Substring(4, 2));
        var rollingId = ssn.Substring(7, 3);
        var checksum = ssn[10];

        if (!CenturyMap.TryGetValue(centuryId, out var centuryBase))
        {
            throw new ArgumentException("Not valid SSN format", nameof(ssn));
        }

        var fullYear = centuryBase + year;
        var maxDay = DaysInGivenMonth(fullYear, month);

        if (month is < 1 or > 12 || day < 1 || day > maxDay)
        {
            throw new ArgumentException("Not valid SSN", nameof(ssn));
        }

        var checksumBase = int.Parse(ssn[..6] + rollingId);
        var expectedChecksum = ChecksumTable[checksumBase % 31];
        var isValid = checksum == expectedChecksum;

        var sex = int.Parse(rollingId) % 2 == 1 ? Male : Female;
        var dateOfBirth = new DateOnly(fullYear, month, day);
        var todayValue = today ?? DateOnly.FromDateTime(DateTime.Today);
        var ageInYears = AgeInYears(dateOfBirth, todayValue);

        return new FinnishSsnParseResult(isValid, sex, ageInYears, dateOfBirth);
    }

    public static bool Validate(string ssn)
    {
        try
        {
            return Parse(ssn).Valid;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
    }

    private static int DaysInGivenMonth(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            return 0;
        }

        var days = DaysInMonth[month - 1];
        if (month == 2 && IsLeapYear(year))
        {
            days++;
        }

        return days;
    }

    private static int AgeInYears(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        return BirthDayPassed(dateOfBirth, today) ? age : age - 1;
    }

    private static bool BirthDayPassed(DateOnly dateOfBirth, DateOnly today)
    {
        return
            dateOfBirth.Month < today.Month
            || (dateOfBirth.Month == today.Month && dateOfBirth.Day <= today.Day);
    }

    [GeneratedRegex(
        "^(0[1-9]|[12]\\d|3[01])(0[1-9]|1[0-2])([5-9]\\d\\+|\\d\\d[-U-Y]|[012]\\d[A-F])\\d{3}[\\dA-Z]$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex SsnRegex();
}

public sealed record FinnishSsnParseResult(
    bool Valid,
    string Sex,
    int AgeInYears,
    DateOnly DateOfBirth
);


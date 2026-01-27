using App.Validation;

namespace App.Validation.Tests;

public sealed class FinnishBusinessIdsTests
{
    [Fact]
    public void IsValidBusinessId_should_fail_for_empty_string()
    {
        Assert.False(FinnishBusinessIds.IsValidBusinessId(string.Empty));
    }

    [Fact]
    public void IsValidBusinessId_should_fail_for_too_short_id()
    {
        Assert.False(FinnishBusinessIds.IsValidBusinessId("261741-4"));
    }

    [Fact]
    public void IsValidBusinessId_should_fail_for_too_long_id()
    {
        Assert.False(FinnishBusinessIds.IsValidBusinessId("2617416-44"));
    }

    [Fact]
    public void IsValidBusinessId_should_pass_for_known_valid_ids()
    {
        var knownValidBusinessIds = new[]
        {
            "1790235-0",
            "1643256-1",
            "0114162-2",
            "1633241-3",
            "2617416-4",
            "1629284-5",
            "1008663-7",
            "0109862-8",
            "1837954-9",
        };

        foreach (var businessId in knownValidBusinessIds)
        {
            Assert.True(FinnishBusinessIds.IsValidBusinessId(businessId));
        }
    }

    [Fact]
    public void IsValidBusinessId_should_fail_when_checksum_remainder_is_one()
    {
        Assert.False(FinnishBusinessIds.IsValidBusinessId("1375045-1"));
    }

    [Fact]
    public void IsValidVatNumber_should_fail_for_empty_string()
    {
        Assert.False(FinnishBusinessIds.IsValidVatNumber(string.Empty));
    }

    [Fact]
    public void IsValidVatNumber_should_fail_for_nonsense_at_end()
    {
        Assert.False(FinnishBusinessIds.IsValidVatNumber("FI26174164A"));
    }

    [Fact]
    public void IsValidVatNumber_should_fail_for_nonsense_at_beginning()
    {
        Assert.False(FinnishBusinessIds.IsValidVatNumber("AFI26174164"));
    }

    [Fact]
    public void IsValidVatNumber_should_pass_for_known_valid_numbers()
    {
        var knownValidVatNumbers = new[]
        {
            "FI17902350",
            "FI16432561",
            "FI01141622",
            "FI16332413",
            "FI26174164",
            "FI16292845",
            "FI10086637",
            "FI01098628",
            "FI18379549",
        };

        foreach (var vatNumber in knownValidVatNumbers)
        {
            Assert.True(FinnishBusinessIds.IsValidVatNumber(vatNumber));
        }
    }

    [Fact]
    public void GenerateBusinessId_should_create_valid_random_ids()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var generated = FinnishBusinessIds.GenerateBusinessId();
            Assert.True(FinnishBusinessIds.IsValidBusinessId(generated));
        }
    }

    [Fact]
    public void GenerateVatNumber_should_create_valid_random_numbers()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var generated = FinnishBusinessIds.GenerateVatNumber();
            Assert.True(FinnishBusinessIds.IsValidVatNumber(generated));
        }
    }

    [Fact]
    public void CalculateChecksum_should_return_expected_value()
    {
        Assert.Equal(5, FinnishBusinessIds.CalculateChecksum("1629284"));
    }
}


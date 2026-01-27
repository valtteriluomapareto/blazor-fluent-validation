namespace App.Contracts;

public sealed class ValidationExamplesForm
{
    public string OptionalFinnishSsn { get; set; } = string.Empty;
    public string RequiredFinnishSsn { get; set; } = string.Empty;

    public string OptionalBusinessId { get; set; } = string.Empty;
    public string RequiredBusinessId { get; set; } = string.Empty;

    public string OptionalIban { get; set; } = string.Empty;
    public string RequiredIban { get; set; } = string.Empty;

    public string OptionalEmail { get; set; } = string.Empty;
    public string RequiredEmail { get; set; } = string.Empty;

    // Locale-aware decimal inputs are modeled as strings so we can validate Finnish formatting.
    public string OptionalDecimalFi { get; set; } = string.Empty;
    public string RequiredDecimalFi { get; set; } = string.Empty;

    // Currency inputs are modeled as strings to allow currency symbols and spacing.
    public string OptionalEurAmount { get; set; } = string.Empty;
    public string RequiredEurAmount { get; set; } = string.Empty;

    // Percentage inputs are modeled as strings to allow optional '%' suffixes.
    public string OptionalPercentage { get; set; } = string.Empty;
    public string RequiredPercentage { get; set; } = string.Empty;

    public IndustryType SentinelIndustry { get; set; } = IndustryType.Unknown;
    public IndustryType? NullableIndustry { get; set; }

    public SingleChoiceOption OptionalSingleChoice { get; set; } = SingleChoiceOption.None;
    public SingleChoiceOption RequiredSingleChoice { get; set; } = SingleChoiceOption.None;
    public string OptionalSingleChoiceOther { get; set; } = string.Empty;
    public string RequiredSingleChoiceOther { get; set; } = string.Empty;

    public List<MultiChoiceOption> OptionalMultiChoice { get; set; } = [];
    public List<MultiChoiceOption> RequiredMultiChoice { get; set; } = [];
    public string OptionalMultiChoiceOther { get; set; } = string.Empty;
    public string RequiredMultiChoiceOther { get; set; } = string.Empty;
}

public enum SingleChoiceOption
{
    None = 0,
    Alpha = 1,
    Beta = 2,
    Gamma = 3,
    Other = 4,
}

public enum MultiChoiceOption
{
    Alpha = 1,
    Beta = 2,
    Gamma = 3,
    Other = 4,
}

namespace App.Contracts;

public sealed class CustomerIntakeForm
{
    public string CustomerName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string SocialSecurityNumber { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public int Seats { get; set; }
    public decimal EstimatedAnnualValue { get; set; }
    public DateOnly ExpectedStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Notes { get; set; } = string.Empty;
    public ContractType ContractType { get; set; } = ContractType.Unknown;
    public IndustryType Industry { get; set; } = IndustryType.Unknown;
}

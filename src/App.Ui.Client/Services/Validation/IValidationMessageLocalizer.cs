namespace FormValidationTest.Client.Services.Validation;

public interface IValidationMessageLocalizer
{
    string Localize(string errorCode, string? fallbackMessage = null);

    IReadOnlyList<string> LocalizeMany(
        IReadOnlyList<string> errorCodes,
        IReadOnlyList<string>? fallbackMessages = null
    );
}

namespace FormValidationTest.Client.Services.Forms;

public sealed record FormSubmitResult(bool HasValidationErrors, string Message);

using App.Contracts;

namespace App.Api.Validation;

internal static class ValidationErrorResponseFactory
{
    private const string DefaultTitle = "Validation failed.";
    private const string DefaultType = "https://httpstatuses.com/400";

    public static ValidationErrorResponse Create(
        HttpContext httpContext,
        Dictionary<string, string[]> errors,
        Dictionary<string, string[]> errorCodes,
        string? title = null,
        string? detail = null
    )
    {
        return new ValidationErrorResponse(
            title ?? DefaultTitle,
            StatusCodes.Status400BadRequest,
            errors,
            errorCodes,
            Type: DefaultType,
            Detail: detail,
            Instance: httpContext.Request.Path
        );
    }
}

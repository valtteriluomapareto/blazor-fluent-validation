namespace App.Contracts;

public sealed record ValidationErrorResponse(
    string Title,
    int Status,
    Dictionary<string, string[]> Errors,
    Dictionary<string, string[]> ErrorCodes,
    string? Type = null,
    string? Detail = null,
    string? Instance = null
);

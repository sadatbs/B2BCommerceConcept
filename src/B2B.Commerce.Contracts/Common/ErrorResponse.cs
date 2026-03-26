namespace B2B.Commerce.Contracts.Common;

public record ErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }

    public static ErrorResponse NotFound(string resource, string identifier) =>
        new() { Code = "NOT_FOUND", Message = $"{resource} with identifier '{identifier}' was not found." };

    public static ErrorResponse Conflict(string message) =>
        new() { Code = "CONFLICT", Message = message };

    public static ErrorResponse ValidationFailed(IDictionary<string, string[]> errors) =>
        new() { Code = "VALIDATION_FAILED", Message = "One or more validation errors occurred.", Errors = errors };
}

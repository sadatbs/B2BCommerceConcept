using System.Text.Json;
using B2B.Commerce.Contracts.Common;

namespace B2B.Commerce.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest,
                "INVALID_OPERATION", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR", "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Code = code,
            Message = message
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

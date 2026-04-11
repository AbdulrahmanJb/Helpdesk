using Helpdesk.Api.Models;
using System.Text.Json;

namespace Helpdesk.Api.Middleware;

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
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred while processing the request.");
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = "An unexpected error occurred.",
            Detail = exception.Message,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

namespace Helpdesk.Api.Models;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public string? TraceId { get; set; }
}

using Helpdesk.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Extensions;

public static class ControllerErrorResponseExtensions
{
    public static IActionResult ApiUnauthorized(this ControllerBase controller, string message)
    {
        return controller.Unauthorized(CreateErrorResponse(controller, StatusCodes.Status401Unauthorized, message));
    }

    public static IActionResult ApiNotFound(this ControllerBase controller, string message)
    {
        return controller.NotFound(CreateErrorResponse(controller, StatusCodes.Status404NotFound, message));
    }

    public static IActionResult ApiBadRequest(this ControllerBase controller, string message, string? detail = null)
    {
        return controller.BadRequest(CreateErrorResponse(controller, StatusCodes.Status400BadRequest, message, detail));
    }

    public static IActionResult ApiForbidden(this ControllerBase controller, string message)
    {
        return controller.StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse(controller, StatusCodes.Status403Forbidden, message));
    }

    private static ApiErrorResponse CreateErrorResponse(ControllerBase controller, int statusCode, string message, string? detail = null)
    {
        return new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Detail = detail,
            TraceId = controller.HttpContext.TraceIdentifier
        };
    }
}

using Helpdesk.Api.Middleware;

namespace Helpdesk.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

namespace PhilisaAbantuBethu.Api;

/// <summary>
/// The one place an API error is turned into a response (W2). Expected refusals (4xx) are logged
/// as warnings and faults (5xx) as errors, so the logs separate "someone sent something wrong"
/// from "the site is broken". Only the method, path and status are logged: never form contents (W7).
///
/// It sits inside the pages' error handler and rethrows anything that isn't an /api request, so a
/// page still gets the HTML error page with PAB's numbers on it.
/// </summary>
public class ApiErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiErrorMiddleware> _log;

    public ApiErrorMiddleware(RequestDelegate next, ILogger<ApiErrorMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (ApiProblem.IsApiPath(context.Request.Path))
        {
            if (context.Response.HasStarted)
            {
                // Too late to change the response; at least say what happened.
                _log.LogError(exception, "{Method} {Path} failed after the response had started",
                    context.Request.Method, context.Request.Path);
                throw;
            }

            var problem = ApiProblem.For(exception, out var status);
            problem.Instance = context.Request.Path;

            if (status >= 500)
            {
                _log.LogError(exception, "{Method} {Path} failed", context.Request.Method, context.Request.Path);
            }
            else
            {
                _log.LogWarning("{Method} {Path} refused with {Status}",
                    context.Request.Method, context.Request.Path, status);
            }

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, options: null,
                contentType: "application/problem+json", context.RequestAborted);
        }
    }
}

public static class ApiErrorExtensions
{
    public static IApplicationBuilder UseApiErrors(this IApplicationBuilder app) =>
        app.UseMiddleware<ApiErrorMiddleware>();
}

using System.Diagnostics;

namespace PhilisaAbantuBethu.Security;

/// <summary>
/// One line per request: method, path, status and how long it took. Deliberately no addresses,
/// no query string and nothing from a form, because a log is the easiest place for someone's
/// situation to end up where it shouldn't be (W7). The rate limiter holds an address only for as
/// long as its window, and never writes it down.
///
/// A path that carries something identifying — a request's id or reference — is reduced to its
/// shape, so /api/support-requests/41 is logged as /api/support-requests/{id}.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _log;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Images, CSS and scripts would drown everything else out.
        if (IsAsset(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var started = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            var ms = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var status = context.Response.StatusCode;
            var path = Shape(context.Request.Path);

            // A broken page is an error; a refused or missing one is only worth a warning.
            if (status >= 500)
            {
                _log.LogError("{Method} {Path} {Status} in {Ms:F0}ms", context.Request.Method, path, status, ms);
            }
            else if (status >= 400)
            {
                _log.LogWarning("{Method} {Path} {Status} in {Ms:F0}ms", context.Request.Method, path, status, ms);
            }
            else
            {
                _log.LogInformation("{Method} {Path} {Status} in {Ms:F0}ms", context.Request.Method, path, status, ms);
            }
        }
    }

    private static bool IsAsset(PathString path) =>
        path.StartsWithSegments("/css") || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/img") || path.StartsWithSegments("/lib")
        || path.Value is "/favicon.ico";

    /// <summary>Replaces the parts of a path that identify one person's request with a placeholder.</summary>
    internal static string Shape(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrEmpty(value)) return "/";

        var segments = value.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            // An id, or a reference like PAB-7KQ39M.
            if (segments[i].Length > 0
                && (segments[i].All(char.IsAsciiDigit) || segments[i].StartsWith("PAB-", StringComparison.OrdinalIgnoreCase)))
            {
                segments[i] = "{id}";
            }
        }
        return string.Join('/', segments);
    }
}

public static class RequestLoggingExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();
}

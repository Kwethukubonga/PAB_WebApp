using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PhilisaAbantuBethu.Api;

/// <summary>
/// Thrown when a request can't be served and the visitor should be told why. Anything else that
/// goes wrong becomes a plain 500 without details, so an internal message never reaches the
/// outside (W2).
/// </summary>
public class ApiException : Exception
{
    public ApiException(int statusCode, string message) : base(message) => StatusCode = statusCode;

    public int StatusCode { get; }

    public static ApiException NotFound(string what) => new(StatusCodes.Status404NotFound, what);
    public static ApiException BadRequest(string why) => new(StatusCodes.Status400BadRequest, why);
}

/// <summary>
/// Every API error comes back in the same shape: RFC 9457 ProblemDetails, as
/// <c>{ "title": …, "status": …, "detail": …, "errors": { field: message } }</c>. The site's own
/// pages keep their HTML error pages; this is for anything under /api.
/// </summary>
public static class ApiProblem
{
    public const string Prefix = "/api";

    /// <summary>A 400 naming each field that was wrong, in the visitor's language.</summary>
    public static ValidationProblem Validation(IDictionary<string, string> errors, string title = "One or more fields need attention.") =>
        TypedResults.ValidationProblem(
            errors.ToDictionary(e => e.Key, e => new[] { e.Value }),
            title: title);

    public static ProblemHttpResult Problem(int status, string title, string? detail = null) =>
        TypedResults.Problem(statusCode: status, title: title, detail: detail);

    /// <summary>Turns an <see cref="ApiException"/> into its ProblemDetails, 5xx into a plain 500.</summary>
    public static ProblemDetails For(Exception exception, out int status)
    {
        if (exception is ApiException api)
        {
            status = api.StatusCode;
            return new ProblemDetails { Status = status, Title = api.Message };
        }

        status = StatusCodes.Status500InternalServerError;
        return new ProblemDetails
        {
            Status = status,
            Title = "Something went wrong on our side.",
            Detail = "Please try again. If you need help now, call the crisis line on +27 (0)81 746 9889."
        };
    }

    public static bool IsApiPath(PathString path) =>
        path.StartsWithSegments(Prefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gives a refusal that carries no body of its own - a 401, 403 or 404 from the pipeline
    /// rather than from an endpoint - the same JSON shape as every other API error.
    /// </summary>
    public static async Task WriteStatusAsync(StatusCodeContext context)
    {
        var response = context.HttpContext.Response;
        var status = response.StatusCode;

        response.StatusCode = status;

        // A 400 with no body of its own means the body couldn't be read at all: broken JSON, the
        // wrong shape, or a field of the wrong type. Say so, so the caller knows what to fix (G7).
        var request = context.HttpContext.Request;
        var unreadableBody = status == StatusCodes.Status400BadRequest && (HttpMethods.IsPost(request.Method) || HttpMethods.IsPatch(request.Method));

        await response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = unreadableBody ? "The request body could not be read." : TitleFor(status),
            Detail = unreadableBody ? "Send a JSON object with the documented fields and types (see /openapi/v1.json); for example, \"declaration\" is true or false." : null,
            Instance = request.Path
        }, options: null, contentType: "application/problem+json", context.HttpContext.RequestAborted);
    }

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Sign in to read this.",
        StatusCodes.Status403Forbidden => "This is for PAB staff only.",
        StatusCodes.Status404NotFound => "There is nothing at this address.",
        StatusCodes.Status405MethodNotAllowed => "That is not something you can do here.",
        _ => "The request could not be completed."
    };
}

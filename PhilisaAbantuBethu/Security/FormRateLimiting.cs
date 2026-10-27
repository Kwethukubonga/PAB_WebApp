using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace PhilisaAbantuBethu.Security;

/// <summary>How many times one address may send the public forms. Set under "RateLimiting" in appsettings.</summary>
public class FormRateLimitOptions
{
    public const string Section = "RateLimiting";

    public int FormPostsPerWindow { get; set; } = 5;
    public int WindowMinutes { get; set; } = 10;
}

/// <summary>
/// Marks a page or endpoint whose sends are limited: the two public forms and the API endpoints
/// that save the same two forms, so going through /api is no way around it.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LimitFormSendsAttribute : Attribute;

public static class FormRateLimiting
{
    public static IServiceCollection AddFormRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<FormRateLimitOptions>(config.GetSection(FormRateLimitOptions.Section));

        return services.AddRateLimiter(limiter =>
        {
            // An empty 429 lets the status code page explain it, with the crisis line on it.
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, ct) =>
            {
                var options = Settings(context.HttpContext);
                var retryAfter = options.WindowMinutes * 60;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                // API callers get the same ProblemDetails shape as every other API error (W2).
                if (Api.ApiProblem.IsApiPath(context.HttpContext.Request.Path))
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests from this address.",
                        Detail = $"Please wait {options.WindowMinutes} minutes and try again. If you need help now, call the crisis line on +27 (0)81 746 9889.",
                        Instance = context.HttpContext.Request.Path
                    }, options: null, contentType: "application/problem+json", ct);
                }
            };

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Only sending these forms is limited. Reading any page never is. The check uses the
                // endpoint routing matched rather than the address, so "/Contact/" or "/CONTACT"
                // can't get round it (routing runs before this middleware).
                var isFormPost = HttpMethods.IsPost(context.Request.Method)
                    && context.GetEndpoint()?.Metadata.GetMetadata<LimitFormSendsAttribute>() is not null;
                if (!isFormPost) return RateLimitPartition.GetNoLimiter("reads");

                var options = Settings(context);
                var address = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(address, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.FormPostsPerWindow,
                    Window = TimeSpan.FromMinutes(options.WindowMinutes),
                    QueueLimit = 0
                });
            });
        });
    }

    private static FormRateLimitOptions Settings(HttpContext context) =>
        context.RequestServices.GetRequiredService<IOptions<FormRateLimitOptions>>().Value;
}

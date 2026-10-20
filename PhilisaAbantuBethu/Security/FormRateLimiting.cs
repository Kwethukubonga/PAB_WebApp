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

public static class FormRateLimiting
{
    // Only sending these forms is limited. Reading any page never is.
    private static readonly string[] LimitedPaths = { "/RequestSupport", "/Contact" };

    public static IServiceCollection AddFormRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<FormRateLimitOptions>(config.GetSection(FormRateLimitOptions.Section));

        return services.AddRateLimiter(limiter =>
        {
            // An empty 429 lets the status code page explain it, with the crisis line on it.
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = (context, _) =>
            {
                var options = Settings(context.HttpContext);
                context.HttpContext.Response.Headers.RetryAfter = (options.WindowMinutes * 60).ToString();
                return ValueTask.CompletedTask;
            };

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var isFormPost = HttpMethods.IsPost(context.Request.Method)
                    && LimitedPaths.Any(path => context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
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

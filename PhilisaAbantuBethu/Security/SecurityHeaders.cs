using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Security;

/// <summary>
/// The headers Express's helmet sets, tuned for this site. Someone may be looking for help in
/// secret, so the address of a page is never passed to the next site they visit, and the site
/// can't be framed by another one.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _contentSecurityPolicy;

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<FirebaseOptions> firebase)
    {
        _next = next;
        var authDomain = firebase.Value.ResolvedAuthDomain;

        // Tailwind and Google Fonts are loaded from CDNs today, and the staff sign-in uses Firebase's
        // own scripts and Google's sign-in window. Inline styles are needed by the Tailwind CDN.
        _contentSecurityPolicy = string.Join("; ",
            "default-src 'self'",
            "script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://www.gstatic.com https://apis.google.com",
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com",
            "img-src 'self' data: https:",
            "connect-src 'self' https://identitytoolkit.googleapis.com https://securetoken.googleapis.com https://www.googleapis.com",
            $"frame-src https://{authDomain} https://accounts.google.com",
            "form-action 'self'",
            "frame-ancestors 'none'",
            "base-uri 'self'",
            "object-src 'none'");
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["Content-Security-Policy"] = _contentSecurityPolicy;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            // Allows Google's sign-in window on the staff page to report back.
            headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
            return Task.CompletedTask;
        });
        return _next(context);
    }
}

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

        // The stylesheet and both fonts are served from this site now, so the Tailwind CDN and
        // Google Fonts are no longer allowed at all (H1, H3). 'unsafe-inline' has gone from
        // script-src with them: every script here is a file of ours. It stays on style-src only
        // because a few elements carry a style attribute for a programme's own colour.
        //
        // What is still allowed from elsewhere is the staff sign-in: Firebase's own scripts and
        // Google's sign-in window.
        _contentSecurityPolicy = string.Join("; ",
            "default-src 'self'",
            "script-src 'self' https://www.gstatic.com https://apis.google.com",
            "style-src 'self' 'unsafe-inline'",
            "font-src 'self'",
            "img-src 'self' data: https:",
            "connect-src 'self' https://identitytoolkit.googleapis.com https://securetoken.googleapis.com https://www.googleapis.com",
            $"frame-src https://{authDomain} https://accounts.google.com",
            // The offline copy of the emergency numbers is kept by a worker from this site (W11).
            "worker-src 'self'",
            "manifest-src 'self'",
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

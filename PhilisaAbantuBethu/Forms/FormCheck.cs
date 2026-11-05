using Microsoft.AspNetCore.Antiforgery;

namespace PhilisaAbantuBethu.Forms;

public enum FormCheckResult { Valid, Expired, Unreadable }

/// <summary>
/// The anti-forgery check for the public forms, done by the page rather than the framework so a
/// refusal becomes a message on the form instead of an error page (G7).
/// </summary>
public static class FormCheck
{
    public static async Task<FormCheckResult> CheckAsync(IAntiforgery antiforgery, HttpContext http)
    {
        try
        {
            return await antiforgery.IsRequestValidAsync(http) ? FormCheckResult.Valid : FormCheckResult.Expired;
        }
        catch (AntiforgeryValidationException)
        {
            // ASP.NET Core refuses to read a form that contains a null character at all, so there
            // is nothing to keep. The browser removes those before sending; this is the fallback.
            return FormCheckResult.Unreadable;
        }
    }
}

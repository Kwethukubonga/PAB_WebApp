using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Pages;

/// <summary>
/// "Your request has been received". Shown only straight after a request is saved: the reference
/// comes through TempData (an encrypted cookie, read once), not the address. A bookmark, a typed
/// link or a link someone sends can't show a made-up reference, and the reference stays out of the
/// browser history of a phone someone else may look at (issue #24).
/// </summary>
public class ConfirmationModel : PageModel
{
    public const string ReferenceKey = "Confirmation.Reference";
    public const string ContactMethodKey = "Confirmation.ContactMethod";

    public string Reference { get; private set; } = "";

    /// <summary>How the person asked to be reached, so the page can say it back to them (G6).</summary>
    public string ContactMethod { get; private set; } = "";

    /// <summary>Today's date as the person reads it, e.g. 27 October 2026, or 27 Oktober 2026.</summary>
    public string SubmittedOn { get; private set; } = "";

    // South Africa has no daylight saving, so SAST is always two hours ahead of UTC.
    private static readonly TimeSpan Sast = TimeSpan.FromHours(2);

    /// <summary>
    /// The date in South African time and in the chosen language. The server runs on UTC, so the
    /// plain clock gave yesterday's date between midnight and 02:00, always in English (#27).
    /// </summary>
    internal static string DateFor(DateTime utc, string lang)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo(lang switch
        {
            "xh" => "xh-ZA",
            "af" => "af-ZA",
            _ => "en-ZA"
        });
        return (DateTime.SpecifyKind(utc, DateTimeKind.Utc) + Sast).ToString("d MMMM yyyy", culture);
    }

    public IActionResult OnGet()
    {
        if (TempData[ReferenceKey] is not string reference || string.IsNullOrWhiteSpace(reference))
        {
            // Nothing was just sent: offer the form rather than a success message.
            return RedirectToPage("/RequestSupport");
        }

        Reference = reference;
        ContactMethod = TempData[ContactMethodKey] as string ?? "";
        SubmittedOn = DateFor(DateTime.UtcNow, HttpContext.Strings().Lang);

        // Like the staff pages: Back or a shared phone's cache shouldn't bring the page up again.
        Response.Headers.CacheControl = "no-store";
        return Page();
    }
}

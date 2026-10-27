using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    public IActionResult OnGet()
    {
        if (TempData[ReferenceKey] is not string reference || string.IsNullOrWhiteSpace(reference))
        {
            // Nothing was just sent: offer the form rather than a success message.
            return RedirectToPage("/RequestSupport");
        }

        Reference = reference;
        ContactMethod = TempData[ContactMethodKey] as string ?? "";

        // Like the staff pages: Back or a shared phone's cache shouldn't bring the page up again.
        Response.Headers.CacheControl = "no-store";
        return Page();
    }
}

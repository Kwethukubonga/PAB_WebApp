using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Pages;

// The form's longest field is capped at 3000 characters, so the whole post fits comfortably in
// 64KB. Kestrel's default 30MB would otherwise let someone force a 30MB allocation per post, and
// with the rate limit only 5 posts a window, that is a cheap way to lean on the server's memory.
[RequestSizeLimit(64 * 1024)]
public class RequestSupportModel : PageModel
{

    private readonly ISupportRequestService _requests;

    public RequestSupportModel(ISupportRequestService requests)
    {

        _requests = requests;

    }

    [BindProperty] public string? FirstName { get; set; }
    [BindProperty] public string? Surname { get; set; }
    [BindProperty] public string? Phone { get; set; }
    [BindProperty] public string? Email { get; set; }
    [BindProperty] public string? Area { get; set; }
    [BindProperty] public string? ContactMethod { get; set; } = "phone";
    [BindProperty] public string? SupportType { get; set; }
    [BindProperty] public string? Situation { get; set; }
    [BindProperty] public string? Urgent { get; set; } = "no";
    [BindProperty] public string? Extra { get; set; }
    [BindProperty] public bool Declaration { get; set; }

    // Hidden from people, so only bots fill it in.
    [BindProperty] public string? Website { get; set; }

    /// <summary>A message for the whole form, shown above the fields.</summary>
    public string? FormError { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {

        var s = HttpContext.Strings();

        if (!string.IsNullOrEmpty(Website))
        {
            // Nothing is saved. A real person who somehow filled it in still gets a way to reach PAB.
            FormError = s["RequestSupport.CouldntSendRequestPlease"];
            return Page();
        }

        var input = new SupportRequestInput(
            FirstName, Surname, Phone, Email, Area, ContactMethod,
            SupportType, Situation, Urgent, Extra, Declaration);

        foreach (var (field, message) in SupportRequestValidator.Validate(input, s))
        {
            ModelState.AddModelError(field, message);
        }

        if (!ModelState.IsValid)
        {
            FormError = s["RequestSupport.PleaseCheckHighlightedFields"];
            return Page();
        }

        var reference = await _requests.SubmitAsync(input, HttpContext.RequestAborted);

        // "by" lets the confirmation page say back how PAB will reach them (G6). It is the
        // choice they just made, not anything identifying.
        return RedirectToPage("/Confirmation", new { reference, by = ContactMethod });

    }

    /// <summary>The message for one field, or null when it has none.</summary>
    public string? ErrorFor(string field) =>
        ModelState.TryGetValue(field, out var entry) && entry.Errors.Count > 0
            ? entry.Errors[0].ErrorMessage
            : null;

}

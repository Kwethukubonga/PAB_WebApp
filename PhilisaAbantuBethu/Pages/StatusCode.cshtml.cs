using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhilisaAbantuBethu.Pages;

/// <summary>
/// Shown for errors without a page of their own, such as a missing page (404) or too many form
/// sends (429). It always gives a way to reach PAB, since someone may be asking for help.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class StatusCodeModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Code { get; set; }

    public void OnGet() { }

    // The re-executed request may be a POST (for example a refused form), so it answers those too.
    public void OnPost() { }
}

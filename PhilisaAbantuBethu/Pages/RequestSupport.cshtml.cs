using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhilisaAbantuBethu.Pages;

public class RequestSupportModel : PageModel
{
    [BindProperty] public string? FirstName { get; set; }
    [BindProperty] public string? Surname { get; set; }
    [BindProperty] public string? Phone { get; set; }
    [BindProperty] public string? Email { get; set; }
    [BindProperty] public string? Area { get; set; }
    [BindProperty] public string? ContactMethod { get; set; }
    [BindProperty] public string? SupportType { get; set; }
    [BindProperty] public string? Situation { get; set; }
    [BindProperty] public string? Urgent { get; set; }
    [BindProperty] public string? Extra { get; set; }
    [BindProperty] public bool Declaration { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        // Placeholder until the database lands — swap this for a real save + generated reference.
        var reference = "PAB" + Random.Shared.Next(100, 1000);

        return RedirectToPage("/Confirmation", new { reference });
    }
}

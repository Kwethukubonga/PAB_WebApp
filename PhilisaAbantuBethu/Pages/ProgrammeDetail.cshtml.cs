using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Pages;

/// <summary>One programme's page, at /Programmes/{id}.</summary>
public class ProgrammeDetailModel : PageModel
{
    public Programme Programme { get; private set; } = null!;

    public IActionResult OnGet(string id)
    {
        // An address that isn't a programme gets the site's own 404 page, with PAB's contact
        // details, instead of another programme's page (#25).
        if (SiteData.FindProgramme(id) is not { } programme) return NotFound();

        Programme = programme;
        return Page();
    }
}

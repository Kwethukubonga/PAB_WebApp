using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Pages;

public class RequestSupportModel : PageModel
{

    private readonly AppDbContext _db;

    public RequestSupportModel(AppDbContext db)
    {

        _db = db;

    }

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

    public async Task<IActionResult> OnPostAsync()
    {

        if (!ModelState.IsValid) 
            return Page();

        var reference = "PAB" + Random.Shared.Next(100, 1000);

        var request = new SupportRequest
        {

            FirstName = FirstName ?? "",
            Surname = Surname ?? "",
            Phone = Phone ?? "",
            Email = Email ?? "",
            Area = Area ?? "",
            ContactMethod = ContactMethod ?? "",
            SupportType = SupportType ?? "",
            Situation = Situation ?? "",
            Urgent = Urgent ?? "",
            Extra = Extra ?? "",
            Declaration = Declaration,
            Reference = reference,
            SubmittedAt = DateTime.Now

        };

        _db.SupportRequests.Add(request);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Confirmation", new { reference = request.Reference });

    }

}

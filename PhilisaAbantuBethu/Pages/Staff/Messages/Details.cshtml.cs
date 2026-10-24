using Microsoft.AspNetCore.Mvc;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Pages.Staff.Messages;

/// <summary>One contact message in full.</summary>
public class DetailsModel : StaffPageModel
{
    private readonly IContactService _contact;
    private readonly ILogger<DetailsModel> _log;

    public DetailsModel(IContactService contact, ILogger<DetailsModel> log)
    {
        _contact = contact;
        _log = log;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public ContactInquiry? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Message = await _contact.FindAsync(Id, HttpContext.RequestAborted);
        if (Message is null) return NotFound();

        // Who opened what, by id only, the same record the support requests keep.
        _log.LogInformation("Staff member {Staff} opened contact message {Id}", StaffAuth.DisplayName(User), Id);
        return Page();
    }
}

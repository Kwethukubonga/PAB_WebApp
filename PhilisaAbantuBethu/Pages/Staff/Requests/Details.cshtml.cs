using Microsoft.AspNetCore.Mvc;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Pages.Staff.Requests;

/// <summary>One support request in full, where staff move it along.</summary>
public class DetailsModel : StaffPageModel
{
    private readonly ISupportRequestService _requests;
    private readonly ILogger<DetailsModel> _log;

    public DetailsModel(ISupportRequestService requests, ILogger<DetailsModel> log)
    {
        _requests = requests;
        _log = log;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public SupportRequest? SupportRequest { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SupportRequest = await _requests.FindAsync(Id, HttpContext.RequestAborted);
        if (SupportRequest is null) return NotFound();

        // Who opened what, by id only, so there's a record without copying the details into logs.
        _log.LogInformation("Staff member {Staff} opened support request {Id}", StaffAuth.DisplayName(User), Id);
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(string status)
    {
        var changed = await _requests.SetStatusAsync(Id, status, StaffAuth.DisplayName(User), HttpContext.RequestAborted);
        if (!changed) return NotFound();

        var s = HttpContext.Strings();
        StatusMessage = s.Pick("Status updated to", "Imeko itshintshelwe ku-", "Status opgedateer na") + " " + StaffText.StatusLabel(s, status) + ".";
        return RedirectToPage(new { id = Id });
    }
}

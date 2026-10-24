using Microsoft.AspNetCore.Mvc;
using PhilisaAbantuBethu.Forms;
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

    /// <summary>What staff have done about it, kept with the request so the next person sees it.</summary>
    [BindProperty]
    public string? StaffNotes { get; set; }

    public async Task<IActionResult> OnPostStatusAsync(string status)
    {
        var s = HttpContext.Strings();

        if (StaffNotes?.Length > SupportRequestValidator.StaffNotesMax)
        {
            StatusMessage = s.Format("Staff.Details.PleaseKeepNotesUnder", SupportRequestValidator.StaffNotesMax);
            return RedirectToPage(new { id = Id });
        }

        var changed = await _requests.SetStatusAsync(
            Id, status, StaffAuth.DisplayName(User), StaffNotes ?? "", HttpContext.RequestAborted);
        if (changed is null) return NotFound();

        StatusMessage = s["Staff.Details.StatusUpdated"] + " " + StaffText.StatusLabel(s, status) + ".";
        return RedirectToPage(new { id = Id });
    }
}

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

    /// <summary>True while the page is asking "are you sure?" before a request is closed (W12).</summary>
    public bool ConfirmingClose { get; private set; }

    /// <summary>Why the notes weren't saved. The page is shown again with the notes as typed.</summary>
    public string? NotesError { get; private set; }

    /// <summary>The notes box shows what was just typed rather than what is saved.</summary>
    public bool KeepTypedNotes => ConfirmingClose || NotesError is not null;

    public async Task<IActionResult> OnPostStatusAsync(string status, bool confirm = false)
    {
        var s = HttpContext.Strings();

        // Only the three buttons' values are real. Anything else is a hand-made post, and gets a
        // message rather than the "not found" page, which would suggest the request was gone (#27).
        if (!SupportRequestStatus.All.Contains(status))
        {
            StatusMessage = s["Staff.Details.UnknownStatus"];
            return RedirectToPage(new { id = Id });
        }

        // Counted the way the database counts (G7), and shown again rather than redirected, so
        // a long note isn't thrown away (#27).
        if (TextInput.Length(StaffNotes ?? "") > SupportRequestValidator.StaffNotesMax)
        {
            SupportRequest = await _requests.FindAsync(Id, HttpContext.RequestAborted);
            if (SupportRequest is null) return NotFound();
            NotesError = s.Format("Staff.Details.PleaseKeepNotesUnder", SupportRequestValidator.StaffNotesMax);
            return Page();
        }

        // Closing takes a request off the working lists, so ask first. The question is a page of
        // its own rather than a script pop-up, so it can't be skipped, and the notes typed so far
        // come back in the box to be saved with the close.
        if (status == SupportRequestStatus.Closed && !confirm)
        {
            SupportRequest = await _requests.FindAsync(Id, HttpContext.RequestAborted);
            if (SupportRequest is null) return NotFound();
            ConfirmingClose = true;
            return Page();
        }

        var changed = await _requests.SetStatusAsync(
            Id, status, StaffAuth.DisplayName(User), StaffNotes ?? "", HttpContext.RequestAborted);
        if (changed is null) return NotFound();

        StatusMessage = s["Staff.Details.StatusUpdated"] + " " + StaffText.StatusLabel(s, status) + ".";
        return RedirectToPage(new { id = Id });
    }
}

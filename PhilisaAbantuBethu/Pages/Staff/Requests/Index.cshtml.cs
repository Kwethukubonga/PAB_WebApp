using Microsoft.AspNetCore.Mvc;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Pages.Staff.Requests;

/// <summary>Support requests for staff, one tab per status, urgent ones first.</summary>
public class IndexModel : StaffPageModel
{
    private readonly ISupportRequestService _requests;

    public IndexModel(ISupportRequestService requests)
    {
        _requests = requests;
    }

    [BindProperty(SupportsGet = true)]
    public string Status { get; set; } = SupportRequestStatus.New;

    public IReadOnlyList<SupportRequest> Requests { get; private set; } = Array.Empty<SupportRequest>();
    public Dictionary<string, int> Counts { get; private set; } = new();

    public async Task OnGetAsync()
    {
        if (!SupportRequestStatus.All.Contains(Status)) Status = SupportRequestStatus.New;
        var ct = HttpContext.RequestAborted;
        Counts = await _requests.CountByStatusAsync(ct);
        Requests = await _requests.ListAsync(Status, ct);
    }
}

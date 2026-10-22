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

    /// <summary>
    /// "oldest" puts the longest wait first, which is the order to work through a queue in.
    /// "newest" answers the other question staff ask: what has just come in. Urgent requests
    /// stay at the top either way (W12).
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = OldestFirst;

    public const string OldestFirst = "oldest";
    public const string NewestFirst = "newest";

    public IReadOnlyList<SupportRequest> Requests { get; private set; } = Array.Empty<SupportRequest>();
    public Dictionary<string, int> Counts { get; private set; } = new();

    public async Task OnGetAsync()
    {
        if (!SupportRequestStatus.All.Contains(Status)) Status = SupportRequestStatus.New;
        if (Sort != NewestFirst) Sort = OldestFirst;

        var ct = HttpContext.RequestAborted;
        Counts = await _requests.CountByStatusAsync(ct);

        var found = await _requests.ListAsync(Status, ct);
        Requests = Sort == NewestFirst
            ? found.OrderByDescending(r => r.IsUrgent).ThenByDescending(r => r.SubmittedAt).ToList()
            : found;
    }
}

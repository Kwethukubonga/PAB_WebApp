using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Services;

public interface ISupportRequestService
{
    /// <summary>Saves a request that has already passed <see cref="SupportRequestValidator"/> and returns its reference.</summary>
    Task<string> SubmitAsync(SupportRequestInput input, CancellationToken ct = default);

    Task<IReadOnlyList<SupportRequest>> ListAsync(string status, CancellationToken ct = default);

    /// <summary>One page of requests for the API, newest-urgent first, with the total count.</summary>
    Task<(IReadOnlyList<SupportRequest> Items, int Total)> PageAsync(
        string? status, bool? urgent, int page, int pageSize, CancellationToken ct = default);

    Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken ct = default);
    Task<SupportRequest?> FindAsync(int id, CancellationToken ct = default);

    /// <summary>Moves a request along. Returns the saved request, or null when there is no such request.</summary>
    Task<SupportRequest?> SetStatusAsync(int id, string status, string handledBy, string? staffNotes = null, CancellationToken ct = default);
}

public class SupportRequestService : ISupportRequestService
{
    // A clash is roughly one in a hundred million, so a few tries is plenty.
    private const int MaxReferenceAttempts = 5;

    /// <summary>
    /// The same request sent again within this time is treated as the one already saved: a double
    /// tap, the Back button followed by Submit, or a phone retrying on a weak signal (G7).
    /// </summary>
    internal static readonly TimeSpan DuplicateWindow = TimeSpan.FromMinutes(10);

    // One submission at a time, so two copies arriving together can't both pass the duplicate
    // check. Requests are rare enough that the wait is never noticed. This holds within one
    // running copy of the site, which is how it is hosted.
    private static readonly SemaphoreSlim SubmitLock = new(1, 1);

    private readonly AppDbContext _db;
    private readonly IReferenceGenerator _references;
    private readonly IStaffAlerts _alerts;
    private readonly ILogger<SupportRequestService> _log;

    public SupportRequestService(AppDbContext db, IReferenceGenerator references, IStaffAlerts alerts, ILogger<SupportRequestService> log)
    {
        _db = db;
        _references = references;
        _alerts = alerts;
        _log = log;
    }

    public async Task<string> SubmitAsync(SupportRequestInput input, CancellationToken ct = default)
    {
        var request = new SupportRequest
        {
            FirstName = input.FirstName!.Trim(),
            Surname = input.Surname!.Trim(),
            Phone = PhoneNumber.Tidy(input.Phone),
            Email = input.Email?.Trim() ?? "",
            Area = input.Area!.Trim(),
            ContactMethod = input.ContactMethod!,
            SupportType = input.SupportType!,
            Situation = input.Situation!.Trim(),
            Urgent = input.Urgent!,
            Extra = input.Extra?.Trim() ?? "",
            Declaration = input.Declaration,
            SubmittedAt = DateTime.UtcNow,
            Status = SupportRequestStatus.New
        };

        await SubmitLock.WaitAsync(ct);
        try
        {
            var earlier = await SameRequestRecentlyAsync(request, ct);
            if (earlier is not null)
            {
                // The person gets the reference they were already given; PAB isn't emailed twice.
                _log.LogInformation("Support request {Id} sent again; kept the first copy", earlier.Id);
                return earlier.Reference;
            }
            return await SaveAsync(request, ct);
        }
        finally
        {
            SubmitLock.Release();
        }
    }

    /// <summary>
    /// A request saved in the last few minutes with the same details, or null. The situation is
    /// encrypted in the database, so the likely matches are narrowed by name, phone and type there,
    /// and the text itself is compared here, once it has been decrypted.
    /// </summary>
    private async Task<SupportRequest?> SameRequestRecentlyAsync(SupportRequest request, CancellationToken ct)
    {
        var since = request.SubmittedAt - DuplicateWindow;
        var candidates = await _db.SupportRequests.AsNoTracking()
            .Where(r => r.SubmittedAt >= since
                && r.Phone == request.Phone
                && r.FirstName == request.FirstName
                && r.Surname == request.Surname
                && r.SupportType == request.SupportType)
            .ToListAsync(ct);

        return candidates.FirstOrDefault(r =>
            r.Situation == request.Situation && r.Extra == request.Extra
            && r.Urgent == request.Urgent && r.Area == request.Area
            && r.ContactMethod == request.ContactMethod && r.Email == request.Email);
    }

    private async Task<string> SaveAsync(SupportRequest request, CancellationToken ct)
    {
        _db.SupportRequests.Add(request);

        for (var attempt = 1; ; attempt++)
        {
            request.Reference = await UnusedReferenceAsync(ct);
            try
            {
                await _db.SaveChangesAsync(ct);
                // Ids and references only: the details stay out of the logs.
                _log.LogInformation("Support request {Id} saved as {Reference}", request.Id, request.Reference);

                // Only once it's safely saved, and never with the person's cancel token: if they
                // close the tab, PAB should still hear about it (W10).
                await _alerts.SupportRequestReceivedAsync(request, CancellationToken.None);
                return request.Reference;
            }
            catch (DbUpdateException) when (attempt < MaxReferenceAttempts)
            {
                // Another request took the same reference between the check and the save.
                _log.LogWarning("Reference clash on attempt {Attempt}, drawing a new one", attempt);
            }
        }
    }

    private async Task<string> UnusedReferenceAsync(CancellationToken ct)
    {
        for (var i = 0; i < MaxReferenceAttempts; i++)
        {
            var candidate = _references.Next();
            if (!await _db.SupportRequests.AnyAsync(r => r.Reference == candidate, ct)) return candidate;
        }
        throw new InvalidOperationException("Could not find an unused reference number.");
    }

    // Urgent requests first, then the oldest, so nothing waiting long gets buried.
    public async Task<IReadOnlyList<SupportRequest>> ListAsync(string status, CancellationToken ct = default) =>
        await _db.SupportRequests
            .AsNoTracking()
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.Urgent == "yes")
            .ThenBy(r => r.SubmittedAt)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<SupportRequest> Items, int Total)> PageAsync(
        string? status, bool? urgent, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.SupportRequests.AsNoTracking();
        if (status is not null) query = query.Where(r => r.Status == status);
        if (urgent is not null)
        {
            var wanted = urgent.Value ? "yes" : "no";
            query = query.Where(r => r.Urgent == wanted);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.Urgent == "yes")
            .ThenBy(r => r.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken ct = default)
    {
        var counts = await _db.SupportRequests
            .GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return SupportRequestStatus.All.ToDictionary(s => s, s => counts.GetValueOrDefault(s));
    }

    public Task<SupportRequest?> FindAsync(int id, CancellationToken ct = default) =>
        _db.SupportRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<SupportRequest?> SetStatusAsync(
        int id, string status, string handledBy, string? staffNotes = null, CancellationToken ct = default)
    {
        if (!SupportRequestStatus.All.Contains(status)) return null;
        var request = await _db.SupportRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request is null) return null;

        request.Status = status;
        request.StatusUpdatedAt = DateTime.UtcNow;
        request.HandledBy = handledBy;
        // Leaving the notes out of the call keeps whatever is already there.
        if (staffNotes is not null) request.StaffNotes = staffNotes.Trim() is { Length: > 0 } notes ? notes : null;
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Support request {Id} moved to {Status} by {Staff}", id, status, handledBy);
        return request;
    }
}

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

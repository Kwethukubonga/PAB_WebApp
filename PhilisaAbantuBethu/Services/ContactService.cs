using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Services;

public interface IContactService
{
    /// <summary>Saves a message that has already passed <see cref="ContactValidator"/>.</summary>
    Task SubmitAsync(ContactInput input, CancellationToken ct = default);

    /// <summary>Messages for staff, newest first.</summary>
    Task<IReadOnlyList<ContactInquiry>> ListAsync(CancellationToken ct = default);

    Task<ContactInquiry?> FindAsync(int id, CancellationToken ct = default);
}

public class ContactService : IContactService
{
    // The retention cleanup keeps this to about a year of messages; the cap is a backstop.
    private const int ListLimit = 500;

    private readonly AppDbContext _db;
    private readonly IStaffAlerts _alerts;
    private readonly ILogger<ContactService> _log;

    public ContactService(AppDbContext db, IStaffAlerts alerts, ILogger<ContactService> log)
    {
        _db = db;
        _alerts = alerts;
        _log = log;
    }

    public async Task SubmitAsync(ContactInput input, CancellationToken ct = default)
    {
        var inquiry = new ContactInquiry
        {
            Name = input.Name!.Trim(),
            Email = input.Email?.Trim() ?? "",
            Phone = PhoneNumber.Tidy(input.Phone),
            Subject = input.Subject!,
            Message = input.Message!.Trim(),
            SubmittedAt = DateTime.UtcNow
        };
        _db.ContactInquiries.Add(inquiry);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Contact message {Id} saved", inquiry.Id);

        await _alerts.ContactMessageReceivedAsync(inquiry, CancellationToken.None);
    }

    public async Task<IReadOnlyList<ContactInquiry>> ListAsync(CancellationToken ct = default) =>
        await _db.ContactInquiries.AsNoTracking()
            .OrderByDescending(c => c.SubmittedAt)
            .Take(ListLimit)
            .ToListAsync(ct);

    public Task<ContactInquiry?> FindAsync(int id, CancellationToken ct = default) =>
        _db.ContactInquiries.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
}

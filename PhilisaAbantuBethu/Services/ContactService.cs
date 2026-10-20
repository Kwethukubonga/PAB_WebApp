using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Services;

public interface IContactService
{
    /// <summary>Saves a message that has already passed <see cref="ContactValidator"/>.</summary>
    Task SubmitAsync(ContactInput input, CancellationToken ct = default);
}

public class ContactService : IContactService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactService> _log;

    public ContactService(AppDbContext db, ILogger<ContactService> log)
    {
        _db = db;
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
    }
}

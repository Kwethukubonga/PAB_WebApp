using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Pages.Staff.Messages;

/// <summary>Messages sent through the Contact page, newest first, so the email alerts have somewhere to point (W10).</summary>
public class IndexModel : StaffPageModel
{
    private readonly IContactService _contact;

    public IndexModel(IContactService contact)
    {
        _contact = contact;
    }

    public IReadOnlyList<ContactInquiry> Messages { get; private set; } = Array.Empty<ContactInquiry>();

    public async Task OnGetAsync()
    {
        Messages = await _contact.ListAsync(HttpContext.RequestAborted);
    }
}

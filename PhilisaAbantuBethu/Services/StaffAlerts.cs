using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Services;

public interface IStaffAlerts
{
    /// <summary>Tells PAB a support request has come in. Never throws: a failed email must not lose the request.</summary>
    Task SupportRequestReceivedAsync(SupportRequest request, CancellationToken ct = default);

    /// <summary>Tells PAB a message has come in through the Contact page. Never throws either.</summary>
    Task ContactMessageReceivedAsync(ContactInquiry message, CancellationToken ct = default);
}

/// <summary>
/// Emails PAB's inbox when a support request comes in, so staff know to look (W10). The email is
/// a nudge, not a copy: it carries the reference, the kind of help, whether it's urgent and when
/// it arrived. Names, contact details, the area and what the person wrote stay behind the staff
/// sign-in, because inboxes get forwarded, synced to phones and read over shoulders.
/// </summary>
public class StaffAlerts : IStaffAlerts
{
    // Long enough for a slow connection, short enough that the person isn't left waiting.
    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(8);

    private readonly IEmailSender _email;
    private readonly AlertOptions _options;
    private readonly ILogger<StaffAlerts> _log;

    public StaffAlerts(IEmailSender email, IOptions<AlertOptions> options, ILogger<StaffAlerts> log)
    {
        _email = email;
        _options = options.Value;
        _log = log;
    }

    public Task SupportRequestReceivedAsync(SupportRequest request, CancellationToken ct = default) =>
        _options.SupportRequests
            ? SendAsync(ForSupportRequest(request, _options.SiteUrl), $"support request {request.Reference}", ct)
            : Task.CompletedTask;

    public Task ContactMessageReceivedAsync(ContactInquiry message, CancellationToken ct = default) =>
        _options.ContactMessages
            ? SendAsync(ForContactMessage(message, _options.SiteUrl), $"contact message {message.Id}", ct)
            : Task.CompletedTask;

    /// <param name="about">What the email is about, for the log: an id or reference, never a name.</param>
    private async Task SendAsync(EmailMessage email, string about, CancellationToken ct)
    {
        if (!_options.IsConfigured)
        {
            _log.LogDebug("Email alerts aren't set up, so none was sent for {About}", about);
            return;
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(SendTimeout);
            await _email.SendAsync(email, timeout.Token);
            _log.LogInformation("Emailed PAB about {About}", about);
        }
        catch (Exception ex)
        {
            // It's already saved and on the staff pages, so this is worth a warning, not an error
            // page. The reference or id is enough to find it; nothing personal goes in the log.
            _log.LogWarning("Could not email PAB about {About}: {Problem}", about, ex.GetType().Name);
        }
    }

    /// <summary>The alert for one request. Only the fields named here can ever reach an inbox.</summary>
    internal static EmailMessage ForSupportRequest(SupportRequest request, string siteUrl)
    {
        var type = EnglishSupportType(request.SupportType);
        var subject = request.IsUrgent
            ? $"New support request · URGENT · {type}"
            : $"New support request · {type}";

        var link = string.IsNullOrWhiteSpace(siteUrl)
            ? "Open Staff → Support requests on the website to see it."
            : $"Open it here: {siteUrl.TrimEnd('/')}/Staff/Requests/Details/{request.Id}";

        var text = string.Join("\n", new[]
        {
            "A new support request has come in through the PAB Help Centre.",
            "",
            $"Reference:  {request.Reference}",
            $"Type:       {type}",
            $"Urgent:     {(request.IsUrgent ? "Yes" : "No")}",
            $"Received:   {StaffText.LocalTime(request.SubmittedAt)}",
            "",
            link,
            "You'll need to sign in with your PAB admin account.",
            "",
            "The person's name, contact details and what they wrote are left out of this email on purpose, to keep them safe."
        });

        return new EmailMessage(subject, text);
    }

    /// <summary>
    /// The alert for a contact message: which of the fixed subjects it was and when. The sender's
    /// name, email, phone number and what they wrote stay on the staff Messages page.
    /// </summary>
    internal static EmailMessage ForContactMessage(ContactInquiry message, string siteUrl)
    {
        var subject = ContactValidator.EnglishSubject(message.Subject);

        var link = string.IsNullOrWhiteSpace(siteUrl)
            ? "Open Staff → Messages on the website to read it."
            : $"Read it here: {siteUrl.TrimEnd('/')}/Staff/Messages/Details/{message.Id}";

        var text = string.Join("\n", new[]
        {
            "A new message has come in through the Contact page.",
            "",
            $"Subject:    {subject}",
            $"Received:   {StaffText.LocalTime(message.SubmittedAt)}",
            "",
            link,
            "You'll need to sign in with your PAB admin account.",
            "",
            "Who sent it and what they wrote are left out of this email on purpose."
        });

        return new EmailMessage($"New message through the contact form · {subject}", text);
    }

    /// <summary>
    /// Someone who used the isiXhosa or Afrikaans form picked the support type in that language.
    /// Staff get the English name, so every alert reads the same way.
    /// </summary>
    internal static string EnglishSupportType(string supportType)
    {
        foreach (var lang in new[] { SiteStrings.En, SiteStrings.Xh, SiteStrings.Af })
        {
            var index = Array.IndexOf(lang.SupportTypes, supportType);
            if (index >= 0 && index < SiteStrings.En.SupportTypes.Length) return SiteStrings.En.SupportTypes[index];
        }
        return supportType;
    }
}

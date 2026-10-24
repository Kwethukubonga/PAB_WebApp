using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Services;

public interface IStaffAlerts
{
    /// <summary>Tells PAB a support request has come in. Never throws: a failed email must not lose the request.</summary>
    Task SupportRequestReceivedAsync(SupportRequest request, CancellationToken ct = default);
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

    public async Task SupportRequestReceivedAsync(SupportRequest request, CancellationToken ct = default)
    {
        if (!_options.SupportRequests) return;
        if (!_options.IsConfigured)
        {
            _log.LogDebug("Email alerts aren't set up, so none was sent for {Reference}", request.Reference);
            return;
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(SendTimeout);
            await _email.SendAsync(ForSupportRequest(request, _options.SiteUrl), timeout.Token);
            _log.LogInformation("Emailed PAB about support request {Reference}", request.Reference);
        }
        catch (Exception ex)
        {
            // The request is already saved and on the staff list, so this is worth a warning, not
            // an error page. The reference is enough to find it; nothing personal goes in the log.
            _log.LogWarning("Could not email PAB about support request {Reference}: {Problem}", request.Reference, ex.GetType().Name);
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

using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace PhilisaAbantuBethu.Services;

/// <summary>
/// Where staff alerts go, set under "Alerts" in appsettings (on Railway: Alerts__ApiKey and so
/// on). With no API key nothing is sent, so a laptop or a test run never emails PAB by accident.
/// </summary>
public class AlertOptions
{
    public const string Section = "Alerts";

    /// <summary>The Resend API key. A secret: only ever a Railway variable or user-secrets, never in git.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Who the email is from, e.g. "PAB Help Centre &lt;alerts@example.org&gt;". The domain must be verified in Resend.</summary>
    public string From { get; set; } = "";

    /// <summary>PAB's intake inbox. Several addresses can be separated with commas.</summary>
    public string To { get; set; } = "";

    /// <summary>The live site's address, used for the link to the staff area, e.g. https://pab.up.railway.app</summary>
    public string SiteUrl { get; set; } = "";

    /// <summary>Email PAB about each new support request.</summary>
    public bool SupportRequests { get; set; } = true;

    /// <summary>Email PAB about each new message through the Contact page.</summary>
    public bool ContactMessages { get; set; } = true;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To);

    public string[] Recipients =>
        To.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

/// <summary>A plain-text email. Kept to a subject and body so nothing else can carry personal details.</summary>
public record EmailMessage(string Subject, string Text);

public interface IEmailSender
{
    /// <summary>Sends the message to the alert recipients, or throws if the email service refuses it.</summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

/// <summary>
/// Sends through Resend's HTTP API. Railway blocks the usual email ports (SMTP) on its cheaper
/// plans, so an HTTPS call to an email service is what works from there.
/// </summary>
public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly AlertOptions _options;

    public ResendEmailSender(HttpClient http, IOptions<AlertOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = _options.From,
                to = _options.Recipients,
                subject = message.Subject,
                text = message.Text
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"The email service answered {(int)response.StatusCode}.", null, response.StatusCode);
    }
}

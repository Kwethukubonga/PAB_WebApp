using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>The email PAB gets about a new support request (W10).</summary>
public class StaffAlertsTests
{
    // Every personal field is distinctive, so the test can prove none of them reach the email.
    private static SupportRequest Request(string supportType = "Emergency Shelter", string urgent = "yes") => new()
    {
        Id = 42,
        Reference = "PAB-7K2Q9M",
        FirstName = "Zanele",
        Surname = "Ndabeni",
        Phone = "082 555 0199",
        Email = "zanele.private@example.org",
        Area = "Lavender Hill",
        ContactMethod = "whatsapp",
        SupportType = supportType,
        Situation = "My partner hurt me last night",
        Urgent = urgent,
        Extra = "Please only message after 6pm",
        Declaration = true,
        SubmittedAt = new DateTime(2026, 10, 24, 12, 32, 0, DateTimeKind.Utc)
    };

    private static AlertOptions Options(bool supportRequests = true, string apiKey = "key") => new()
    {
        ApiKey = apiKey, From = "alerts@pab.test", To = "intake@pab.test", SiteUrl = "https://pab.test/", SupportRequests = supportRequests
    };

    private static StaffAlerts Alerts(RecordingEmailSender sender, AlertOptions? options = null) =>
        new(sender, Microsoft.Extensions.Options.Options.Create(options ?? Options()), NullLogger<StaffAlerts>.Instance);

    [Fact]
    public void TheEmail_SaysWhichRequest_AndHowUrgent()
    {
        var email = StaffAlerts.ForSupportRequest(Request(), "https://pab.test/");

        Assert.Equal("New support request · URGENT · Emergency Shelter", email.Subject);
        Assert.Contains("PAB-7K2Q9M", email.Text);
        Assert.Contains("Urgent:     Yes", email.Text);
        // Stored in UTC, shown in Cape Town time.
        Assert.Contains("24 Oct 2026, 14:32", email.Text);
        Assert.Contains("https://pab.test/Staff/Requests/Details/42", email.Text);
    }

    [Fact]
    public void TheEmail_LeavesOutEverythingPersonal()
    {
        var request = Request();
        var email = StaffAlerts.ForSupportRequest(request, "https://pab.test");
        var everything = email.Subject + "\n" + email.Text;

        foreach (var personal in new[] { request.FirstName, request.Surname, request.Phone, request.Email, request.Area, request.Situation, request.Extra, "WhatsApp", "whatsapp" })
            Assert.DoesNotContain(personal, everything);
    }

    [Fact]
    public void ANonUrgentRequest_SaysSo()
    {
        var email = StaffAlerts.ForSupportRequest(Request("Food Assistance", "no"), "");

        Assert.Equal("New support request · Food Assistance", email.Subject);
        Assert.Contains("Urgent:     No", email.Text);
        Assert.Contains("Staff → Support requests", email.Text);
    }

    [Theory]
    [InlineData("Inkxaso Yokutya", "Food Assistance")]
    [InlineData("Voedselbystand", "Food Assistance")]
    [InlineData("Emergency Shelter", "Emergency Shelter")]
    [InlineData("Something else", "Something else")]
    public void TheSupportType_IsAlwaysInEnglish(string chosen, string english) =>
        Assert.Equal(english, StaffAlerts.EnglishSupportType(chosen));

    [Fact]
    public async Task ABrokenEmailService_DoesNotThrow()
    {
        var sender = new RecordingEmailSender { Fail = true };

        await Alerts(sender).SupportRequestReceivedAsync(Request());

        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task NothingIsSent_WhenAlertsArentSetUp()
    {
        var sender = new RecordingEmailSender();

        await Alerts(sender, Options(apiKey: "")).SupportRequestReceivedAsync(Request());

        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task NothingIsSent_WhenSupportAlertsAreTurnedOff()
    {
        var sender = new RecordingEmailSender();

        await Alerts(sender, Options(supportRequests: false)).SupportRequestReceivedAsync(Request());

        Assert.Empty(sender.Sent);
    }
}

/// <summary>What actually goes to Resend, caught before it leaves.</summary>
public class ResendEmailSenderTests
{
    private class CatchingHandler(HttpStatusCode answer) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string Body { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            Body = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(answer);
        }
    }

    private static ResendEmailSender Sender(CatchingHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") },
        Options.Create(new AlertOptions { ApiKey = "re_test", From = "PAB <alerts@pab.test>", To = "intake@pab.test, manager@pab.test" }));

    [Fact]
    public async Task Sends_ToResend_WithTheKeyAndBothRecipients()
    {
        var handler = new CatchingHandler(HttpStatusCode.OK);

        await Sender(handler).SendAsync(new EmailMessage("New support request · Food Assistance", "Reference: PAB-7K2Q9M"));

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://api.resend.com/emails", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer re_test", handler.Request.Headers.Authorization!.ToString());
        Assert.Contains("\"to\":[\"intake@pab.test\",\"manager@pab.test\"]", handler.Body);
        Assert.Contains("\"from\":\"PAB \\u003Calerts@pab.test\\u003E\"", handler.Body);
        Assert.Contains("PAB-7K2Q9M", handler.Body);
    }

    [Fact]
    public async Task Throws_WhenResendRefuses()
    {
        var handler = new CatchingHandler(HttpStatusCode.Forbidden);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            Sender(handler).SendAsync(new EmailMessage("Subject", "Text")));
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }
}

/// <summary>The alert from the form all the way through, with the email service stood in for.</summary>
public class StaffAlertsSiteTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public StaffAlertsSiteTests(SiteFactory site)
    {
        _site = site;
    }

    /// <summary>Sends the form and returns the response with the reference the person was shown.</summary>
    private async Task<(HttpResponseMessage Response, string Reference)> SubmitAsync(string firstName)
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/RequestSupport?lang=xh");
        var response = await browser.PostAsync("/RequestSupport?lang=xh", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FirstName"] = firstName,
            ["Surname"] = "Dlamini",
            ["Phone"] = "071 123 4567",
            ["Email"] = "",
            ["Area"] = "Lavender Hill",
            ["ContactMethod"] = "phone",
            ["SupportType"] = "Inkxaso Yokutya",
            ["Situation"] = "We need help with food this month.",
            ["Urgent"] = "yes",
            ["Extra"] = "",
            ["Declaration"] = "true",
            ["Website"] = ""
        }));

        // The reference is no longer in the redirect address (issue #24), so read it off the page.
        var confirmation = await (await browser.GetAsync("/Confirmation")).Content.ReadAsStringAsync();
        var reference = System.Text.RegularExpressions.Regex.Match(confirmation, "PAB-[2-9A-Z]{6}").Value;
        Assert.NotEmpty(reference);
        return (response, reference);
    }

    [Fact]
    public async Task ANewRequest_EmailsPab()
    {
        var (_, reference) = await SubmitAsync("Nomsa");

        var email = Assert.Single(_site.Emails.Sent, e => e.Text.Contains(reference));
        Assert.Equal("New support request · URGENT · Food Assistance", email.Subject);
        Assert.DoesNotContain("Nomsa", email.Text);
        Assert.DoesNotContain("food this month", email.Text);
    }

    [Fact]
    public async Task ARequest_IsStillSaved_WhenTheEmailFails()
    {
        _site.Emails.Fail = true;
        try
        {
            var (response, reference) = await SubmitAsync("Thandeka");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(_site.WithDb(db => db.SupportRequests.Any(r => r.Reference == reference)));
        }
        finally
        {
            _site.Emails.Fail = false;
        }
    }
}

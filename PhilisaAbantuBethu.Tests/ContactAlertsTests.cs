using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>The email PAB gets about a contact message (W10).</summary>
public class ContactAlertsTests
{
    private static ContactInquiry Message() => new()
    {
        Id = 17,
        Name = "Riaan Botha",
        Email = "riaan.private@example.org",
        Phone = "083 555 0142",
        Subject = "Vrywilligerwerk",
        Message = "I'd like to help on Saturdays",
        SubmittedAt = new DateTime(2026, 10, 24, 8, 15, 0, DateTimeKind.Utc)
    };

    [Fact]
    public void TheEmail_SaysWhatKindOfMessage_AndWhen()
    {
        var email = StaffAlerts.ForContactMessage(Message(), "https://pab.test");

        Assert.Equal("New message through the contact form · Volunteering", email.Subject);
        Assert.Contains("24 Oct 2026, 10:15", email.Text);
        Assert.Contains("https://pab.test/Staff/Messages/Details/17", email.Text);
    }

    [Fact]
    public void TheEmail_LeavesOutWhoSentIt_AndWhatTheyWrote()
    {
        var message = Message();
        var email = StaffAlerts.ForContactMessage(message, "https://pab.test");
        var everything = email.Subject + "\n" + email.Text;

        foreach (var personal in new[] { message.Name, message.Email, message.Phone, message.Message })
            Assert.DoesNotContain(personal, everything);
    }

    [Theory]
    [InlineData("Ukuba Sisithi Ngobuyisi", "Volunteering")]
    [InlineData("Media", "Media")]
    [InlineData("Ander", "Other")]
    [InlineData("Not a subject", "Not a subject")]
    public void TheSubject_IsAlwaysInEnglish(string chosen, string english) =>
        Assert.Equal(english, ContactValidator.EnglishSubject(chosen));

    [Fact]
    public async Task NothingIsSent_WhenContactAlertsAreTurnedOff()
    {
        var sender = new RecordingEmailSender();
        var options = new AlertOptions { ApiKey = "key", From = "a@pab.test", To = "b@pab.test", ContactMessages = false };

        await new StaffAlerts(sender, Microsoft.Extensions.Options.Options.Create(options), NullLogger<StaffAlerts>.Instance)
            .ContactMessageReceivedAsync(Message());

        Assert.Empty(sender.Sent);
    }
}

/// <summary>The contact form, its alert, and the staff Messages pages it points to.</summary>
public class StaffMessagesTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public StaffMessagesTests(SiteFactory site)
    {
        _site = site;
    }

    private async Task<HttpResponseMessage> SendContactAsync(HttpClient browser, string name, string message)
    {
        var token = await SiteFactory.FormTokenAsync(browser, "/Contact");
        return await browser.PostAsync("/Contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Name"] = name,
            ["Email"] = "someone@example.org",
            ["Phone"] = "",
            ["Subject"] = "Media",
            ["Message"] = message,
            ["Website"] = ""
        }));
    }

    private async Task<HttpClient> SignedInAsync()
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/Staff/SignIn");
        await browser.PostAsync("/Staff/SignIn?handler=Session", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["idToken"] = SiteFactory.AdminToken
        }));
        return browser;
    }

    [Fact]
    public async Task AContactMessage_EmailsPab_WithoutTheMessage()
    {
        var response = await SendContactAsync(_site.CreateBrowser(), "Ayesha Daniels", "A question about a news story");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var id = _site.WithDb(db => db.ContactInquiries.Single(c => c.Name == "Ayesha Daniels").Id);
        var email = Assert.Single(_site.Emails.Sent, e => e.Text.Contains($"/Staff/Messages/Details/{id}"));
        Assert.Equal("New message through the contact form · Media", email.Subject);
        Assert.DoesNotContain("Ayesha", email.Text);
        Assert.DoesNotContain("news story", email.Text);
    }

    [Fact]
    public async Task AContactMessage_IsStillSaved_WhenTheEmailFails()
    {
        _site.Emails.Fail = true;
        try
        {
            var response = await SendContactAsync(_site.CreateBrowser(), "Grace Petersen", "Saved even so");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(_site.WithDb(db => db.ContactInquiries.Any(c => c.Name == "Grace Petersen")));
        }
        finally
        {
            _site.Emails.Fail = false;
        }
    }

    [Theory]
    [InlineData("/Staff/Messages")]
    [InlineData("/Staff/Messages/Details/1")]
    public async Task MessagesPages_NeedAStaffSignIn(string url)
    {
        var response = await _site.CreateBrowser().GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Staff/SignIn", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Staff_CanListAndReadMessages_AndTheyreNeverCached()
    {
        await SendContactAsync(_site.CreateBrowser(), "Bongani Mthembu", "Could we partner on a food drive?");
        var id = _site.WithDb(db => db.ContactInquiries.Single(c => c.Name == "Bongani Mthembu").Id);
        var browser = await SignedInAsync();

        var list = await browser.GetAsync("/Staff/Messages");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.True(list.Headers.CacheControl!.NoStore);
        var listHtml = await list.Content.ReadAsStringAsync();
        Assert.Contains("Bongani Mthembu", listHtml);
        Assert.Contains($"/Staff/Messages/Details/{id}", listHtml);

        var details = await browser.GetAsync($"/Staff/Messages/Details/{id}");
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        Assert.True(details.Headers.CacheControl!.NoStore);
        var html = await details.Content.ReadAsStringAsync();
        Assert.Contains("Could we partner on a food drive?", html);
        Assert.Contains("mailto:someone@example.org", html);
    }

    [Fact]
    public async Task AMissingMessage_IsNotFound()
    {
        var browser = await SignedInAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/Staff/Messages/Details/999999")).StatusCode);
    }

    [Fact]
    public async Task TheStaffBar_LinksToMessages_InEachLanguage()
    {
        var browser = await SignedInAsync();

        Assert.Contains(">Messages</a>", await browser.GetStringAsync("/Staff/Requests?lang=en"));
        Assert.Contains(">Imiyalezo</a>", await browser.GetStringAsync("/Staff/Requests?lang=xh"));
        Assert.Contains(">Boodskappe</a>", await browser.GetStringAsync("/Staff/Requests?lang=af"));
    }
}

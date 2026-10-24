using System.Net;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Tests;

/// <summary>Closing a request asks "are you sure?" first, without needing JavaScript (W12).</summary>
public class StaffCloseTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public StaffCloseTests(SiteFactory site)
    {
        _site = site;
    }

    private int AddRequest(string reference, string status = SupportRequestStatus.New) => _site.WithDb(db =>
    {
        var request = new SupportRequest
        {
            FirstName = "Lindiwe", Surname = "Mokoena", Phone = "082 123 4567", Area = "Steenberg",
            ContactMethod = "phone", SupportType = "Food Assistance", Situation = "Private details",
            Urgent = "no", Declaration = true, Reference = reference, SubmittedAt = DateTime.UtcNow, Status = status
        };
        db.SupportRequests.Add(request);
        db.SaveChanges();
        return request.Id;
    });

    private SupportRequest Saved(int id) => _site.WithDb(db => db.SupportRequests.Single(r => r.Id == id));

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

    private static async Task<HttpResponseMessage> MoveAsync(HttpClient browser, int id, string status, string notes, bool? confirm = null, string lang = "en")
    {
        var url = $"/Staff/Requests/Details/{id}?lang={lang}";
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await SiteFactory.FormTokenAsync(browser, url),
            ["status"] = status,
            ["StaffNotes"] = notes
        };
        if (confirm is bool c) form["confirm"] = c ? "true" : "false";
        return await browser.PostAsync($"/Staff/Requests/Details/{id}?handler=Status&lang={lang}", new FormUrlEncodedContent(form));
    }

    [Fact]
    public async Task Closing_AsksFirst_AndChangesNothingYet()
    {
        var id = AddRequest("PAB-CLOSE1");
        var browser = await SignedInAsync();

        var response = await MoveAsync(browser, id, SupportRequestStatus.Closed, "Called her back, all sorted.");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Close PAB-CLOSE1?", html);
        Assert.Contains("Yes, close it", html);
        // What was typed comes back in the box, so it isn't lost while staff decide.
        Assert.Contains("Called her back, all sorted.", html);
        var saved = Saved(id);
        Assert.Equal(SupportRequestStatus.New, saved.Status);
        Assert.Null(saved.StaffNotes);
    }

    [Fact]
    public async Task Confirming_ClosesIt_WithTheNotes()
    {
        var id = AddRequest("PAB-CLOSE2");
        var browser = await SignedInAsync();

        var response = await MoveAsync(browser, id, SupportRequestStatus.Closed, "Referred to the shelter.", confirm: true);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var saved = Saved(id);
        Assert.Equal(SupportRequestStatus.Closed, saved.Status);
        Assert.Equal("Referred to the shelter.", saved.StaffNotes);
    }

    [Fact]
    public async Task OtherMoves_DontAsk()
    {
        var id = AddRequest("PAB-CLOSE3");
        var browser = await SignedInAsync();

        var response = await MoveAsync(browser, id, SupportRequestStatus.InProgress, "");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(SupportRequestStatus.InProgress, Saved(id).Status);
    }

    [Theory]
    [InlineData("xh", "PAB-CLOSXH", "Uyasivala isicelo PAB-CLOSXH?", "Ewe, sivale")]
    [InlineData("af", "PAB-CLOSAF", "Sluit PAB-CLOSAF?", "Ja, sluit dit")]
    public async Task TheQuestion_IsInTheChosenLanguage(string lang, string reference, string question, string yes)
    {
        var id = AddRequest(reference);
        var browser = await SignedInAsync();

        var html = await (await MoveAsync(browser, id, SupportRequestStatus.Closed, "", lang: lang)).Content.ReadAsStringAsync();

        Assert.Contains(question, html);
        Assert.Contains(yes, html);
    }
}

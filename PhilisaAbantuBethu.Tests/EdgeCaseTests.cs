using System.Net;
using System.Text;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>The awkward input real people send: emoji, pasted invisible characters, odd phone formats (G7).</summary>
public class TextInputTests
{
    [Theory]
    [InlineData("help\0me", "helpme")]
    [InlineData("help\u0007\u001bme", "helpme")]
    [InlineData("abc‮def", "abcdef")]             // right-to-left override
    [InlineData("a⁦b⁩c", "abc")]             // direction isolates
    [InlineData("line one\nline two\ttab", "line one\nline two\ttab")]
    [InlineData("Zan\U0001F600ele", "Zan\U0001F600ele")]
    [InlineData("N’wamitwa O'Brien-Ndlôvu", "N’wamitwa O'Brien-Ndlôvu")]
    public void Clean_RemovesOnlyInvisibleTrouble(string typed, string kept) =>
        Assert.Equal(kept, TextInput.Clean(typed));

    [Fact]
    public void Clean_LeavesNothingAlone() => Assert.Null(TextInput.Clean(null));

    [Theory]
    [InlineData("Nomsa", 5)]
    [InlineData("\U0001F600\U0001F600\U0001F600", 3)]
    [InlineData("Ndlôvu", 6)]
    public void Length_CountsCharactersAsPeopleDo(string text, int length) =>
        Assert.Equal(length, TextInput.Length(text));

    [Fact]
    public void FortyEmoji_FitAName()
    {
        var input = Valid() with { FirstName = string.Concat(Enumerable.Repeat("\U0001F600", 40)) };

        Assert.DoesNotContain("FirstName", SupportRequestValidator.Validate(input, SiteStrings.En).Keys);
    }

    [Fact]
    public void SixtyOneCharacters_AreTooLongForAName()
    {
        var input = Valid() with { FirstName = new string('a', 61) };

        Assert.Contains("FirstName", SupportRequestValidator.Validate(input, SiteStrings.En).Keys);
    }

    [Theory]
    [InlineData("+27 82 123 4567", "082 123 4567")]
    [InlineData("+27821234567", "082 123 4567")]
    [InlineData("082-123-4567", "082 123 4567")]
    [InlineData("(082) 123 4567", "082 123 4567")]
    [InlineData("0821234567", "082 123 4567")]
    [InlineData("+27 (0)82 123 4567", "082 123 4567")]
    [InlineData("+49 151  2345 6789", "+49 151 2345 6789")] // other countries keep their own spacing
    [InlineData("", "")]
    public void Tidy_StoresSouthAfricanNumbersOneWay(string typed, string stored) =>
        Assert.Equal(stored, PhoneNumber.Tidy(typed));

    private static SupportRequestInput Valid() => new(
        "Nomsa", "Dlamini", "071 123 4567", "", "Lavender Hill", "phone",
        "Food Assistance", "We need help with food.", "no", "", true);
}

/// <summary>The same awkward input sent through the whole site.</summary>
public class EdgeCaseTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public EdgeCaseTests(SiteFactory site)
    {
        _site = site;
    }

    private static Dictionary<string, string> Form(string token, string firstName) => new()
    {
        ["__RequestVerificationToken"] = token, ["FirstName"] = firstName, ["Surname"] = "Dlamini",
        ["Phone"] = "071 123 4567", ["Email"] = "", ["Area"] = "Lavender Hill", ["ContactMethod"] = "phone",
        ["SupportType"] = "Food Assistance", ["Situation"] = "We need help with food this month.",
        ["Urgent"] = "no", ["Extra"] = "", ["Declaration"] = "true", ["Website"] = ""
    };

    private async Task<(HttpClient Browser, string Token)> OpenFormAsync(string url = "/RequestSupport")
    {
        var browser = _site.CreateBrowser();
        return (browser, await SiteFactory.FormTokenAsync(browser, url));
    }

    private int Saved(string firstName) => _site.WithDb(db => db.SupportRequests.Count(r => r.FirstName == firstName));

    [Fact]
    public async Task ADoubleTap_SavesOneRequest_AndBothGetTheSameReference()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "DoubleTap");

        var responses = await Task.WhenAll(
            browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form)),
            browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Redirect, r.StatusCode));
        Assert.Equal(responses[0].Headers.Location, responses[1].Headers.Location);
        Assert.Equal(1, Saved("DoubleTap"));
    }

    [Fact]
    public async Task GoingBackAndSubmittingAgain_DoesNotSaveASecondCopy()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "BackButton");

        var first = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        var again = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        Assert.Equal(first.Headers.Location, again.Headers.Location);
        Assert.Equal(1, Saved("BackButton"));
    }

    [Fact]
    public async Task ADifferentRequestFromTheSamePerson_IsStillSaved()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "TwoNeeds");
        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        form["SupportType"] = "Emergency Shelter";
        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        Assert.Equal(2, Saved("TwoNeeds"));
    }

    [Fact]
    public async Task AnExpiredForm_KeepsWhatWasTyped_AndSavesNothing()
    {
        // A token from another browser stands in for one that has expired.
        var (_, otherToken) = await OpenFormAsync();
        var form = Form(otherToken, "ExpiredForm");
        form["Situation"] = "A long account of what happened that must not be lost.";

        var response = await _site.CreateBrowser().PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("open for a while", html);
        Assert.Contains("value=\"ExpiredForm\"", html);
        Assert.Contains("A long account of what happened that must not be lost.", html);
        Assert.Equal(0, Saved("ExpiredForm"));
    }

    [Fact]
    public async Task AFormThatCannotBeRead_GetsAMessage_NotAnErrorPage()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "NullCharacter");
        form["Situation"] = "help\0me";

        var response = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("couldn&#x27;t be read", await response.Content.ReadAsStringAsync());
        Assert.Equal(0, Saved("NullCharacter"));
    }

    [Fact]
    public async Task InvisibleCharacters_AreRemovedBeforeSaving()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "Spoof‮of");
        form["Situation"] = "help\u0007 me";

        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        var saved = _site.WithDb(db => db.SupportRequests.Single(r => r.FirstName == "Spoofof"));
        Assert.Equal("help me", saved.Situation);
    }

    [Fact]
    public async Task AScriptTypedIntoTheForm_IsShownToStaffAsText()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "ScriptTest");
        form["Situation"] = "<script>alert(1)</script>";
        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        var id = _site.WithDb(db => db.SupportRequests.Single(r => r.FirstName == "ScriptTest").Id);

        var staff = _site.CreateBrowser();
        var signInToken = await SiteFactory.FormTokenAsync(staff, "/Staff/SignIn");
        await staff.PostAsync("/Staff/SignIn?handler=Session", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = signInToken, ["idToken"] = SiteFactory.AdminToken
        }));
        var html = await staff.GetStringAsync($"/Staff/Requests/Details/{id}");

        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
    }

    [Fact]
    public async Task PhoneNumbers_AreStoredOneWay()
    {
        var (browser, token) = await OpenFormAsync();
        var form = Form(token, "PhoneFormat");
        form["Phone"] = "(082) 123-4567";

        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        Assert.Equal("082 123 4567", _site.WithDb(db => db.SupportRequests.Single(r => r.FirstName == "PhoneFormat").Phone));
    }

    [Fact]
    public async Task AContactMessageSentTwice_IsKeptOnce()
    {
        var (browser, token) = await OpenFormAsync("/Contact");
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token, ["Name"] = "Twice Sent", ["Email"] = "twice@example.org",
            ["Phone"] = "", ["Subject"] = "Media", ["Message"] = "The same message.", ["Website"] = ""
        };

        await browser.PostAsync("/Contact", new FormUrlEncodedContent(form));
        await browser.PostAsync("/Contact", new FormUrlEncodedContent(form));

        Assert.Equal(1, _site.WithDb(db => db.ContactInquiries.Count(c => c.Name == "Twice Sent")));
    }

    [Theory]
    [InlineData("{\"firstName\":")]
    [InlineData("[1,2,3]")]
    [InlineData("{\"firstName\":\"A\",\"declaration\":\"yes\"}")]
    public async Task BrokenJson_ToTheApi_SaysWhatIsWrong(string json)
    {
        var response = await _site.CreateBrowser().PostAsync("/api/support-requests", new StringContent(json, Encoding.UTF8, "application/json"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("could not be read", body);
    }
}

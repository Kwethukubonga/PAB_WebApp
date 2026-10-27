using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Tests;

/// <summary>Requests through the whole site, from the browser's side.</summary>
public class SiteIntegrationTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public SiteIntegrationTests(SiteFactory site)
    {
        _site = site;
    }

    private static Dictionary<string, string> ValidRequest(string token) => new()
    {
        ["__RequestVerificationToken"] = token,
        ["FirstName"] = "Nomsa",
        ["Surname"] = "Dlamini",
        ["Phone"] = "071 123 4567",
        ["Email"] = "",
        ["Area"] = "Lavender Hill",
        ["ContactMethod"] = "phone",
        ["SupportType"] = "Food Assistance",
        ["Situation"] = "We need help with food this month.",
        ["Urgent"] = "yes",
        ["Extra"] = "",
        ["Declaration"] = "true",
        ["Website"] = ""
    };

    [Fact]
    public async Task Health_AnswersHealthy()
    {
        var response = await _site.CreateBrowser().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Alive_AnswersWithoutAskingTheDatabase()
    {
        // The host and the uptime monitor ask this every few minutes, so it must not wake the
        // managed database. A check that always fails stands in for a database that can't be
        // reached: /health runs it and reports the failure, /alive never runs it.
        using var site = new SiteFactory();
        using var withBrokenCheck = site.WithWebHostBuilder(host => host.ConfigureServices(services =>
            services.AddHealthChecks().AddCheck("broken", () => HealthCheckResult.Unhealthy("down"))));
        var client = withBrokenCheck.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/health")).StatusCode);

        var alive = await client.GetAsync("/alive");
        Assert.Equal(HttpStatusCode.OK, alive.StatusCode);
        Assert.Equal("Healthy", await alive.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Pages_SendTheSecurityHeaders()
    {
        var response = await _site.CreateBrowser().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }

    [Fact]
    public async Task MissingPage_StillShowsHowToGetHelp()
    {
        var response = await _site.CreateBrowser().GetAsync("/no-such-page");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("10111", html);
    }

    [Fact]
    public async Task Footer_NoLongerShowsTheMadeUpNpoNumber()
    {
        var html = await _site.CreateBrowser().GetStringAsync("/");

        Assert.DoesNotContain("NPO 123-456", html);
        Assert.Contains("data-quick-exit", html);
    }

    [Fact]
    public async Task RequestSupport_SavesAValidRequest_AndShowsItsReference()
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/RequestSupport");

        var response = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(ValidRequest(token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        // Nothing in the address: the reference travels in TempData (issue #24).
        Assert.Equal("/Confirmation", response.Headers.Location!.OriginalString);

        var confirmation = await browser.GetAsync("/Confirmation");
        var html = await confirmation.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
        Assert.Matches("PAB-[2-9A-Z]{6}", html);
        // It still says back how PAB will be in touch (G6).
        Assert.Contains("by phone", html);
        Assert.Contains("no-store", confirmation.Headers.CacheControl!.ToString());

        // Shown once: opening it again goes back to the form instead of claiming a new request.
        var again = await browser.GetAsync("/Confirmation");
        Assert.Equal(HttpStatusCode.Redirect, again.StatusCode);
        Assert.Equal("/RequestSupport", again.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("/Confirmation")]
    [InlineData("/Confirmation?reference=CALL-0800-FAKE-NUMBER")]
    [InlineData("/Confirmation?reference=PAB-ABCDEF&by=phone")]
    public async Task Confirmation_DoesNotClaimARequest_WhenNoneWasJustSent(string address)
    {
        var browser = _site.CreateBrowser();

        var response = await browser.GetAsync(address);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/RequestSupport", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task RequestSupport_TurnsAwayABadRequest_AndKeepsWhatWasTyped()
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/RequestSupport");
        var form = ValidRequest(token);
        form["FirstName"] = "";
        form["Phone"] = "0888888888";
        form["Surname"] = "Keep-Me";
        var before = _site.WithDb(db => db.SupportRequests.Count());

        var response = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Please enter your first name.", html);
        Assert.Contains("aria-invalid=\"true\"", html);
        Assert.Contains("value=\"Keep-Me\"", html);
        Assert.Equal(before, _site.WithDb(db => db.SupportRequests.Count()));
    }

    [Fact]
    public async Task RequestSupport_RefusesAPostWithoutTheFormToken()
    {
        var form = ValidRequest("not-a-real-token");
        form["FirstName"] = "NoToken";

        var response = await _site.CreateBrowser().PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        // Refused all the same: nothing is saved. The form comes back with a message and what was
        // typed, rather than an error page (G7).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("open for a while", await response.Content.ReadAsStringAsync());
        Assert.False(_site.WithDb(db => db.SupportRequests.Any(r => r.FirstName == "NoToken")));
    }

    [Fact]
    public async Task Staff_PagesSendASignedOutVisitorToSignIn()
    {
        var response = await _site.CreateBrowser().GetAsync("/Staff/Requests");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("http://localhost/Staff/SignIn", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Staff_SignInTurnsAwayAVolunteerAndAnInvalidToken()
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/Staff/SignIn");

        var volunteer = await browser.PostAsync("/Staff/SignIn?handler=Session", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["idToken"] = SiteFactory.VolunteerToken
        }));
        var forged = await browser.PostAsync("/Staff/SignIn?handler=Session", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["idToken"] = "made-up"
        }));

        Assert.Equal(HttpStatusCode.Forbidden, volunteer.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, forged.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await browser.GetAsync("/Staff/Requests")).StatusCode);
    }

    [Fact]
    public async Task Staff_AnAdminCanSeeARequestAndMoveItAlong()
    {
        var id = _site.WithDb(db =>
        {
            var request = new SupportRequest
            {
                FirstName = "Lindiwe", Surname = "Mokoena", Phone = "082 123 4567", Area = "Steenberg",
                ContactMethod = "whatsapp", SupportType = "Food Assistance", Situation = "Private details",
                Urgent = "yes", Declaration = true, Reference = "PAB-TESTAB", SubmittedAt = DateTime.UtcNow
            };
            db.SupportRequests.Add(request);
            db.SaveChanges();
            return request.Id;
        });

        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/Staff/SignIn");
        var signIn = await browser.PostAsync("/Staff/SignIn?handler=Session", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["idToken"] = SiteFactory.AdminToken
        }));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
        Assert.Contains("/Staff/Requests", await signIn.Content.ReadAsStringAsync());

        var list = await browser.GetAsync("/Staff/Requests");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("PAB-TESTAB", await list.Content.ReadAsStringAsync());
        // Requests can describe abuse, so the page must never be kept in a cache.
        Assert.True(list.Headers.CacheControl!.NoStore);

        var detailsUrl = $"/Staff/Requests/Details/{id}";
        var detailsToken = await SiteFactory.FormTokenAsync(browser, detailsUrl);
        var move = await browser.PostAsync($"{detailsUrl}?handler=Status", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = detailsToken,
            ["status"] = SupportRequestStatus.InProgress
        }));

        Assert.Equal(HttpStatusCode.Redirect, move.StatusCode);
        var saved = _site.WithDb(db => db.SupportRequests.Single(r => r.Id == id));
        Assert.Equal(SupportRequestStatus.InProgress, saved.Status);
        Assert.Equal("admin@pab.test", saved.HandledBy);
    }
}

/// <summary>A separate site with a low limit, so these posts can't affect the other tests.</summary>
public class RateLimitTests : IClassFixture<RateLimitTests.LowLimitSite>
{
    public class LowLimitSite : SiteFactory
    {
        public LowLimitSite() { FormPostsPerWindow = 2; }
    }

    private readonly LowLimitSite _site;

    public RateLimitTests(LowLimitSite site)
    {
        _site = site;
    }

    [Fact]
    public async Task RequestSupport_SlowsDownRepeatedSends_AndStillShowsTheCrisisLine()
    {
        var browser = _site.CreateBrowser();
        var token = await SiteFactory.FormTokenAsync(browser, "/RequestSupport");
        var form = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["FirstName"] = "" };

        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));
        var third = await browser.PostAsync("/RequestSupport", new FormUrlEncodedContent(form));

        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.True(third.Headers.Contains("Retry-After"));
        Assert.Contains("+27 (0)81 746 9889", await third.Content.ReadAsStringAsync());

        // Reading pages is never limited.
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("/RequestSupport")).StatusCode);
    }
}

/// <summary>
/// Routing still serves a form at "/Contact/" or "/CONTACT", so the limit has to follow the page,
/// not the exact address. Otherwise a trailing slash gets round it (issue #22).
/// </summary>
public class RateLimitAddressTests
{
    [Theory]
    [InlineData("/RequestSupport/")]
    [InlineData("/Contact/")]
    [InlineData("/api/support-requests/")]
    [InlineData("/api/contact/")]
    [InlineData("/REQUESTSUPPORT")]
    [InlineData("/Api/Contact")]
    public async Task Another_spelling_of_a_form_address_is_limited_too(string path)
    {
        using var site = new SiteFactory { FormPostsPerWindow = 1 };
        var client = site.CreateBrowser();
        // The API only accepts JSON; anything else is turned away by routing, before it can be saved.
        Func<HttpContent> empty = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? () => JsonContent.Create(new { })
            : () => new FormUrlEncodedContent(new Dictionary<string, string>());

        // The first send uses up the one allowed. It only counts if the address reached the form.
        await client.PostAsync(path, empty());
        var second = await client.PostAsync(path, empty());

        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
    }

    [Fact]
    public async Task The_limit_is_shared_between_the_spellings()
    {
        using var site = new SiteFactory { FormPostsPerWindow = 1 };
        var client = site.CreateBrowser();

        await client.PostAsync("/api/support-requests", JsonContent.Create(new { }));
        var refused = await client.PostAsync("/api/support-requests/", JsonContent.Create(new { }));

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
    }
}

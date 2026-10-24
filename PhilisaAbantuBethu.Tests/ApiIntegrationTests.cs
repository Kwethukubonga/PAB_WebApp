using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// The shared API, tested through the real pipeline with a throwaway database (B4, W5). Every
/// endpoint is checked for the status code it gives on success, on bad input, when nobody is
/// signed in, when the caller is not an admin, and when the thing asked for isn't there.
/// </summary>
public class ApiIntegrationTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public ApiIntegrationTests(SiteFactory site) => _site = site;

    private static object ValidRequest(string firstName = "Nomsa") => new
    {
        firstName,
        surname = "Mkoka",
        phone = "082 123 4567",
        email = "",
        area = "Lavender Hill",
        contactMethod = "phone",
        supportType = "Emergency Shelter",
        situation = "I need somewhere safe for tonight.",
        urgent = "yes",
        extra = "",
        declaration = true
    };

    private HttpClient Caller(string? token = null)
    {
        var client = _site.CreateBrowser();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    // ---------- Sending a request: open to anyone ----------

    [Fact]
    public async Task Posting_a_valid_request_returns_201_with_its_reference()
    {
        var response = await Caller().PostAsJsonAsync("/api/support-requests", ValidRequest("Created"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var reference = body.GetProperty("reference").GetString();

        Assert.StartsWith("PAB-", reference);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(reference!, response.Headers.Location!.ToString());

        var saved = _site.WithDb(db => db.SupportRequests.AsNoTracking().Single(r => r.Reference == reference));
        Assert.Equal("Created", saved.FirstName);
        Assert.Equal(SupportRequestStatus.New, saved.Status);
    }

    [Fact]
    public async Task Posting_an_empty_request_returns_400_naming_each_field()
    {
        var response = await Caller().PostAsJsonAsync("/api/support-requests", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        foreach (var field in new[] { "FirstName", "Surname", "Phone", "Area", "SupportType", "Situation", "Urgent", "Declaration" })
        {
            Assert.True(errors.TryGetProperty(field, out _), $"No error reported for {field}");
        }
    }

    [Fact]
    public async Task Posting_a_bad_phone_number_saves_nothing()
    {
        var before = _site.WithDb(db => db.SupportRequests.Count());

        var response = await Caller().PostAsJsonAsync("/api/support-requests", new
        {
            firstName = "Thandi", surname = "Nkosi", phone = "12345", area = "Steenberg",
            contactMethod = "phone", supportType = "Food Assistance",
            situation = "We need food parcels.", urgent = "no", declaration = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, _site.WithDb(db => db.SupportRequests.Count()));
    }

    // ---------- Reading requests: PAB admins only ----------

    [Fact]
    public async Task Listing_requests_without_a_token_returns_401()
    {
        var response = await Caller().GetAsync("/api/support-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listing_requests_as_a_volunteer_returns_403()
    {
        var response = await Caller(SiteFactory.VolunteerToken).GetAsync("/api/support-requests");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Listing_requests_as_an_admin_returns_a_page_without_the_situation_text()
    {
        await Caller().PostAsJsonAsync("/api/support-requests", ValidRequest("Listed"));

        var response = await Caller(SiteFactory.AdminToken).GetAsync("/api/support-requests?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(5, page.GetProperty("pageSize").GetInt32());
        Assert.True(page.GetProperty("total").GetInt32() >= 1);

        var first = page.GetProperty("items").EnumerateArray().First();
        Assert.False(first.TryGetProperty("situation", out _), "A list must not carry the situation text");
        Assert.True(first.TryGetProperty("reference", out _));
    }

    [Fact]
    public async Task An_unknown_status_filter_returns_400()
    {
        var response = await Caller(SiteFactory.AdminToken).GetAsync("/api/support-requests?status=pending");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_urgent_filter_returns_only_urgent_requests()
    {
        await Caller().PostAsJsonAsync("/api/support-requests", ValidRequest("UrgentOne"));

        var response = await Caller(SiteFactory.AdminToken).GetAsync("/api/support-requests?urgent=true&pageSize=100");
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.All(page.GetProperty("items").EnumerateArray(), item => Assert.True(item.GetProperty("urgent").GetBoolean()));
    }

    [Fact]
    public async Task One_request_in_full_includes_the_situation_for_an_admin()
    {
        var id = await NewRequestIdAsync("Detailed");

        var response = await Caller(SiteFactory.AdminToken).GetAsync($"/api/support-requests/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("I need somewhere safe for tonight.", body.GetProperty("situation").GetString());
    }

    [Fact]
    public async Task A_request_that_does_not_exist_returns_404_as_problem_details()
    {
        var response = await Caller(SiteFactory.AdminToken).GetAsync("/api/support-requests/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
    }

    // ---------- Changing a request ----------

    [Fact]
    public async Task An_admin_can_move_a_request_to_another_status()
    {
        var id = await NewRequestIdAsync("Moved");

        var response = await Caller(SiteFactory.AdminToken).PatchAsJsonAsync(
            $"/api/support-requests/{id}", new { status = SupportRequestStatus.InProgress, staffNotes = "Called back." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(SupportRequestStatus.InProgress, body.GetProperty("status").GetString());

        var saved = _site.WithDb(db => db.SupportRequests.AsNoTracking().Single(r => r.Id == id));
        Assert.Equal(SupportRequestStatus.InProgress, saved.Status);
        Assert.Equal("Called back.", saved.StaffNotes);
        Assert.Equal("admin@pab.test", saved.HandledBy);
        Assert.NotNull(saved.StatusUpdatedAt);
    }

    [Fact]
    public async Task A_status_we_do_not_use_is_refused_with_400()
    {
        var id = await NewRequestIdAsync("Refused");

        var response = await Caller(SiteFactory.AdminToken).PatchAsJsonAsync(
            $"/api/support-requests/{id}", new { status = "archived" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Changing_a_request_that_does_not_exist_returns_404()
    {
        var response = await Caller(SiteFactory.AdminToken).PatchAsJsonAsync(
            "/api/support-requests/999999", new { status = SupportRequestStatus.Closed });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Changing_a_request_without_a_token_returns_401()
    {
        var response = await Caller().PatchAsJsonAsync("/api/support-requests/1", new { status = SupportRequestStatus.Closed });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_invalid_token_returns_401()
    {
        var response = await Caller("not-a-real-token").GetAsync("/api/support-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Contact ----------

    [Fact]
    public async Task A_valid_contact_message_returns_204_and_is_saved()
    {
        var response = await Caller().PostAsJsonAsync("/api/contact", new
        {
            name = "Brandon Samuels",
            email = "brandon@example.org",
            phone = "",
            subject = "Volunteering",
            message = "I would like to help on Saturdays."
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(_site.WithDb(db => db.ContactInquiries.Any(c => c.Email == "brandon@example.org")));
    }

    [Fact]
    public async Task A_contact_message_with_no_way_to_reply_returns_400()
    {
        var response = await Caller().PostAsJsonAsync("/api/contact", new
        {
            name = "Anon", email = "", phone = "", subject = "Media", message = "Please call me."
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Public content ----------

    [Fact]
    public async Task Programmes_can_be_read_by_anyone_and_cached()
    {
        var response = await Caller().GetAsync("/api/programmes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.Public);

        var programmes = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEmpty(programmes.EnumerateArray());
    }

    [Theory]
    [InlineData("en", "Women's Empowerment")]
    [InlineData("xh", "Ukuxhobisa Abafazi")]
    public async Task A_programme_comes_back_in_the_language_asked_for(string lang, string title)
    {
        var response = await Caller().GetAsync($"/api/programmes/womens-empowerment?lang={lang}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(title, body.GetProperty("title").GetString());
        Assert.NotEmpty(body.GetProperty("objectives").EnumerateArray());
    }

    [Fact]
    public async Task A_programme_that_does_not_exist_returns_404()
    {
        var response = await Caller().GetAsync("/api/programmes/not-a-programme");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Resources_lead_with_the_emergency_numbers()
    {
        var response = await Caller().GetAsync("/api/resources");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = body.GetProperty("categories").EnumerateArray().First();

        Assert.Equal("emergency", first.GetProperty("id").GetString());
        Assert.Contains("10111", first.GetProperty("items").EnumerateArray().First().GetString());
        Assert.NotEmpty(body.GetProperty("questions").EnumerateArray());
    }

    [Fact]
    public async Task The_api_describes_itself_for_anyone_reading_it()
    {
        var response = await Caller().GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/support-requests", document);
        Assert.Contains("/api/programmes", document);
    }

    // The request-body ceiling (Kestrel's global 256 KB, and 64 KB on the two forms) can't be
    // asserted here: the in-process test host doesn't run Kestrel, so it never enforces a body
    // size and the oversized post is rejected by antiforgery or validation instead of at the
    // limit. It is verified against the real container, where a 300 KB body returns 413.

    private async Task<int> NewRequestIdAsync(string firstName)
    {
        var response = await Caller().PostAsJsonAsync("/api/support-requests", ValidRequest(firstName));
        response.EnsureSuccessStatusCode();
        var reference = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reference").GetString();
        return _site.WithDb(db => db.SupportRequests.AsNoTracking().Single(r => r.Reference == reference).Id);
    }
}

/// <summary>
/// The limit is its own test class because it needs a site that allows only a few sends, and
/// that setting is read when the site starts (A2).
/// </summary>
public class ApiRateLimitTests
{
    [Fact]
    public async Task Too_many_sends_from_one_address_get_429_in_the_same_json_shape()
    {
        using var site = new SiteFactory { FormPostsPerWindow = 2 };
        var client = site.CreateBrowser();

        var body = new
        {
            firstName = "Nomsa", surname = "Mkoka", phone = "082 123 4567", area = "Lavender Hill",
            contactMethod = "phone", supportType = "Emergency Shelter",
            situation = "I need somewhere safe for tonight.", urgent = "yes", declaration = true
        };

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/support-requests", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/support-requests", body)).StatusCode);

        var refused = await client.PostAsJsonAsync("/api/support-requests", body);
        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Equal("application/problem+json", refused.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(refused.Headers.RetryAfter);

        var problem = await refused.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(429, problem.GetProperty("status").GetInt32());
        // Someone in danger must still be told how to get help.
        Assert.Contains("746 9889", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Reading_pages_is_never_limited()
    {
        using var site = new SiteFactory { FormPostsPerWindow = 1 };
        var client = site.CreateBrowser();

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/resources")).StatusCode);
        }
    }

    private static readonly object SupportBody = new
    {
        firstName = "Nomsa", surname = "Mkoka", phone = "082 123 4567", area = "Lavender Hill",
        contactMethod = "phone", supportType = "Emergency Shelter",
        situation = "I need somewhere safe for tonight.", urgent = "yes", declaration = true
    };

    private static Task<HttpResponseMessage> SendAs(HttpClient client, string header, string address, string? forwardedFor = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/support-requests") { Content = JsonContent.Create(SupportBody) };
        request.Headers.Add(header, address);
        if (forwardedFor is not null) request.Headers.Add("X-Forwarded-For", forwardedFor);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task Behind_the_proxy_each_visitor_gets_their_own_limit_from_the_header_the_host_names()
    {
        // The hosted site reads Fly-Client-IP: Fly puts its own address at the right of
        // X-Forwarded-For, so reading that would give every visitor one shared limit.
        using var site = new SiteFactory { FormPostsPerWindow = 1, TrustProxy = true, TrustProxyHeader = "Fly-Client-IP" };
        var client = site.CreateBrowser();

        Assert.Equal(HttpStatusCode.Created, (await SendAs(client, "Fly-Client-IP", "203.0.113.10")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await SendAs(client, "Fly-Client-IP", "203.0.113.10")).StatusCode);

        // A different visitor is not held up by the first one's limit.
        Assert.Equal(HttpStatusCode.Created, (await SendAs(client, "Fly-Client-IP", "203.0.113.11")).StatusCode);
    }

    [Fact]
    public async Task Behind_the_proxy_a_rotating_x_forwarded_for_does_not_get_round_the_limit()
    {
        using var site = new SiteFactory { FormPostsPerWindow = 1, TrustProxy = true, TrustProxyHeader = "Fly-Client-IP" };
        var client = site.CreateBrowser();

        Assert.Equal(HttpStatusCode.Created, (await SendAs(client, "Fly-Client-IP", "203.0.113.20", "198.51.100.1")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await SendAs(client, "Fly-Client-IP", "203.0.113.20", "198.51.100.2")).StatusCode);
    }

    [Fact]
    public async Task Behind_a_host_with_two_proxies_the_visitor_is_the_entry_just_left_of_them()
    {
        // The host's proxies add the last two entries; the visitor is the one before those, and
        // whatever the visitor wrote themselves sits further left.
        using var site = new SiteFactory { FormPostsPerWindow = 1, TrustProxy = true, TrustProxyHops = 2 };
        var client = site.CreateBrowser();

        Assert.Equal(HttpStatusCode.Created, (await ForwardedFor(client, "198.51.100.1, 203.0.113.40, 10.0.0.1")).StatusCode);

        // Same visitor, a different made-up address to the left: still the same limit.
        Assert.Equal(HttpStatusCode.TooManyRequests, (await ForwardedFor(client, "198.51.100.2, 203.0.113.40, 10.0.0.1")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await ForwardedFor(client, "203.0.113.40, 10.0.0.1")).StatusCode);

        // A different visitor is not held up by the first one's limit.
        Assert.Equal(HttpStatusCode.Created, (await ForwardedFor(client, "198.51.100.2, 203.0.113.41, 10.0.0.1")).StatusCode);
    }

    private static Task<HttpResponseMessage> ForwardedFor(HttpClient client, string forwardedFor)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/support-requests") { Content = JsonContent.Create(SupportBody) };
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task Without_a_proxy_the_forwarded_headers_are_ignored()
    {
        // Reached directly (docker compose, a plain docker run) the header is the visitor's to forge.
        using var site = new SiteFactory { FormPostsPerWindow = 1 };
        var client = site.CreateBrowser();

        Assert.Equal(HttpStatusCode.Created, (await SendAs(client, "Fly-Client-IP", "203.0.113.30")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await SendAs(client, "Fly-Client-IP", "203.0.113.31", "198.51.100.3")).StatusCode);
    }
}

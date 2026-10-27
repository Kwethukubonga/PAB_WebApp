using System.Net;
using System.Text.RegularExpressions;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// The things every public page has to get right, whichever page somebody lands on: a way out,
/// the right language, and no requests going to anybody else's server (F1 to F5, G4, H1, H3).
/// </summary>
public class PublicPageTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public PublicPageTests(SiteFactory site) => _site = site;

    public static TheoryData<string> PublicPages() => new()
    {
        "/", "/About", "/Programmes", "/Programmes/womens-empowerment",
        "/Resources", "/Contact", "/RequestSupport", "/Privacy", "/offline"
    };

    [Theory]
    [MemberData(nameof(PublicPages))]
    public async Task Every_public_page_has_the_quick_exit_button(string url)
    {
        var html = await _site.CreateBrowser().GetStringAsync(url);

        // Somebody may need to leave the moment the person hurting them walks in (G4).
        Assert.Contains("data-quick-exit", html);
    }

    [Theory]
    [MemberData(nameof(PublicPages))]
    public async Task No_public_page_asks_another_server_for_anything(string url)
    {
        var html = await _site.CreateBrowser().GetStringAsync(url);

        // The stylesheet and both fonts are served from here now (H1, H3). The pages someone
        // reads because they need help should not be announced to a third party either.
        Assert.DoesNotContain("cdn.tailwindcss.com", html);
        Assert.DoesNotContain("fonts.googleapis.com", html);
        Assert.DoesNotContain("fonts.gstatic.com", html);
    }

    [Theory]
    [InlineData("en", "en-ZA", "Resources")]
    [InlineData("xh", "xh-ZA", "Izibonelelo")]
    [InlineData("af", "af-ZA", "Hulpbronne")]
    public async Task A_page_comes_back_in_the_language_asked_for(string lang, string htmlLang, string heading)
    {
        var html = await _site.CreateBrowser().GetStringAsync($"/Resources?lang={lang}");

        // The lang attribute matters as much as the words: it is what tells a screen reader
        // which language to pronounce (PAB-FR09).
        Assert.Contains($"<html lang=\"{htmlLang}\">", html);
        Assert.Contains(heading, html);
    }

    [Fact]
    public async Task The_chosen_language_is_remembered_for_the_next_page()
    {
        var browser = _site.CreateBrowser();

        await browser.GetStringAsync("/Resources?lang=af");
        var next = await browser.GetStringAsync("/Contact");

        Assert.Contains("<html lang=\"af-ZA\">", next);
    }

    [Theory]
    [MemberData(nameof(PublicPages))]
    public async Task Every_public_page_can_be_opened(string url)
    {
        var response = await _site.CreateBrowser().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task The_form_marks_its_required_fields_without_javascript()
    {
        var html = await _site.CreateBrowser().GetStringAsync("/RequestSupport");

        // Rendered by the server, not added by a script, so the form still says what it needs
        // when the script never runs (F2).
        foreach (var field in new[] { "firstName", "surname", "phone", "area" })
        {
            var input = Regex.Match(html, $"<input id=\"{field}\"[^>]*>", RegexOptions.IgnoreCase);
            Assert.True(input.Success, $"No {field} field on the form");
            Assert.Contains("required", input.Value);
            Assert.Contains("aria-required=\"true\"", input.Value);
        }
    }

    [Fact]
    public async Task The_form_tells_the_browser_what_each_field_is_for()
    {
        var html = await _site.CreateBrowser().GetStringAsync("/RequestSupport");

        // WCAG 1.3.5: a saved address or phone number fills itself in, which is less to type
        // on a phone and less to get wrong (F2).
        Assert.Contains("autocomplete=\"given-name\"", html);
        Assert.Contains("autocomplete=\"family-name\"", html);
        Assert.Contains("autocomplete=\"tel\"", html);
        Assert.Contains("autocomplete=\"address-level2\"", html);
    }

    [Fact]
    public async Task The_form_names_its_step_for_somebody_on_a_phone()
    {
        var html = await _site.CreateBrowser().GetStringAsync("/RequestSupport");

        // The labels beside the circles are hidden below the sm breakpoint, so without this
        // a phone showed three numbered dots and nothing to say what they were (F7).
        Assert.Contains("Step 1 of 3", html);
    }

    [Fact]
    public async Task The_photos_are_lazy_and_say_how_big_they_are()
    {
        var html = await _site.CreateBrowser().GetStringAsync("/Programmes");

        // Every photo below the first screen waits, and each one reserves its own space so the
        // page does not jump as they arrive (H2).
        Assert.Contains("loading=\"lazy\"", html);
        Assert.Contains("type=\"image/webp\"", html);
        Assert.Matches("<img[^>]*width=\"\\d+\"[^>]*height=\"\\d+\"", html);
    }

    [Theory]
    [InlineData("/Programmes/nope")]
    [InlineData("/Programmes/%3Cscript%3E")]
    [InlineData("/Programmes/Womens-Empowerment")]
    public async Task An_address_that_is_not_a_programme_gets_the_404_page(string url)
    {
        var response = await _site.CreateBrowser().GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();

        // Not another programme's page with 200 (#25): the site's own 404, with a way to reach PAB.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("We can&#x27;t find that page", html);
        Assert.Contains("data-quick-exit", html);
    }

    [Fact]
    public async Task A_real_programme_still_opens()
    {
        var response = await _site.CreateBrowser().GetAsync("/Programmes/baby-saver");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Baby Saver", await response.Content.ReadAsStringAsync());
    }
}

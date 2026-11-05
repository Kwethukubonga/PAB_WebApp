using System.Text.RegularExpressions;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// The service worker runs inside a browser, which a test here can't start. What can be checked is
/// what the worker is told to keep and what it is told never to keep (FR15): the emergency numbers
/// stay on the phone, and nothing a person typed, no staff page and no API answer ever does.
/// </summary>
public class ServiceWorkerTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public ServiceWorkerTests(SiteFactory site) => _site = site;

    private Task<string> Worker() => _site.CreateBrowser().GetStringAsync("/sw.js");

    private static string[] KeptFiles(string worker)
    {
        var list = Regex.Match(worker, @"const ESSENTIAL = \[(.*?)\];", RegexOptions.Singleline);
        Assert.True(list.Success, "sw.js no longer has an ESSENTIAL list");
        return Regex.Matches(list.Groups[1].Value, "'([^']+)'").Select(m => m.Groups[1].Value).ToArray();
    }

    [Fact]
    public async Task The_resources_page_and_the_offline_page_are_kept()
    {
        var kept = KeptFiles(await Worker());

        Assert.Contains("/Resources", kept);
        Assert.Contains("/offline", kept);
    }

    [Theory]
    [InlineData("/Staff")]
    [InlineData("/api")]
    [InlineData("/Confirmation")]
    [InlineData("/RequestSupport")]
    [InlineData("/Contact")]
    public async Task These_are_never_kept_on_the_phone(string prefix)
    {
        var worker = await Worker();

        // Not on the list of files saved when the worker installs...
        Assert.DoesNotContain(KeptFiles(worker), file => file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        // ...and named in the check that turns a request away from the cache.
        Assert.Contains($"startsWith('{prefix}')", worker);
    }

    [Fact]
    public async Task Only_plain_reads_of_the_listed_files_are_saved()
    {
        var worker = await Worker();

        // A form send is never handled, a page with a query string (a reference number, say) is
        // never saved, and a file is saved only if it is on the list.
        Assert.Contains("request.method !== 'GET'", worker);
        Assert.Contains("url.search.length > 0", worker);
        Assert.Contains("ESSENTIAL.includes(url.pathname)", worker);
    }
}

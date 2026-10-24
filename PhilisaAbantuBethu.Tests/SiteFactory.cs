using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// Starts the real site, with its own throwaway SQLite file and stand-ins for Google, so the
/// whole request pipeline (validation, rate limiting, headers, staff sign-in) is tested offline.
/// </summary>
public class SiteFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"pab-test-{Guid.NewGuid():N}.db");

    public int FormPostsPerWindow { get; init; } = 1000;

    /// <summary>Behaves as the hosted site does, with a proxy in front that reports the visitor's address.</summary>
    public bool TrustProxy { get; init; }

    /// <summary>The header the proxy puts the visitor's address in; empty means X-Forwarded-For.</summary>
    public string? TrustProxyHeader { get; init; }

    /// <summary>How many entries at the right of X-Forwarded-For belong to the host's own proxies.</summary>
    public int? TrustProxyHops { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("RateLimiting:FormPostsPerWindow", FormPostsPerWindow.ToString());
        builder.UseSetting("Firebase:ApiKey", "test-api-key");
        if (TrustProxy) builder.UseSetting("TrustProxy", "true");
        if (TrustProxyHeader is not null) builder.UseSetting("TrustProxyHeader", TrustProxyHeader);
        if (TrustProxyHops is not null) builder.UseSetting("TrustProxyHops", TrustProxyHops.Value.ToString());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IFirebaseTokenVerifier, FakeTokenVerifier>();
            services.AddSingleton<IStaffDirectory, FakeStaffDirectory>();
        });
    }

    /// <summary>Reads or changes the site's database directly, outside a request.</summary>
    public T WithDb<T>(Func<AppDbContext, T> work)
    {
        using var scope = Services.CreateScope();
        return work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch (IOException) { }
    }

    /// <summary>Opens a page and reads its anti-forgery token, the way a browser would before posting.</summary>
    public static async Task<string> FormTokenAsync(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
        Assert.True(match.Success, $"No form token on {url}");
        return match.Groups[1].Value;
    }

    public const string AdminToken = "admin-token";
    public const string VolunteerToken = "volunteer-token";

    private class FakeTokenVerifier : IFirebaseTokenVerifier
    {
        public Task<FirebaseUser?> VerifyAsync(string idToken, CancellationToken ct = default) =>
            Task.FromResult(idToken switch
            {
                AdminToken => new FirebaseUser("admin-uid", "admin@pab.test", "Thandi Admin"),
                VolunteerToken => new FirebaseUser("volunteer-uid", "volunteer@pab.test", "Sipho Volunteer"),
                _ => (FirebaseUser?)null
            });
    }

    private class FakeStaffDirectory : IStaffDirectory
    {
        public Task<bool> IsStaffAsync(FirebaseUser user, string idToken, CancellationToken ct = default) =>
            Task.FromResult(user.Uid == "admin-uid");
    }
}

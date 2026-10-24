using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Services;
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

    /// <summary>Every alert the site would have emailed, instead of sending it.</summary>
    public RecordingEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("RateLimiting:FormPostsPerWindow", FormPostsPerWindow.ToString());
        builder.UseSetting("Firebase:ApiKey", "test-api-key");
        builder.UseSetting("Alerts:ApiKey", "test-email-key");
        builder.UseSetting("Alerts:From", "alerts@pab.test");
        builder.UseSetting("Alerts:To", "intake@pab.test");
        builder.UseSetting("Alerts:SiteUrl", "https://pab.test");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IFirebaseTokenVerifier, FakeTokenVerifier>();
            services.AddSingleton<IStaffDirectory, FakeStaffDirectory>();
            services.AddSingleton<IEmailSender>(Emails);
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

/// <summary>Keeps the emails the site would have sent, and can be told to fail like a broken email service.</summary>
public class RecordingEmailSender : IEmailSender
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<EmailMessage> _sent = new();

    public bool Fail { get; set; }

    public IReadOnlyList<EmailMessage> Sent => _sent.ToList();

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (Fail) throw new HttpRequestException("The email service answered 500.");
        _sent.Enqueue(message);
        return Task.CompletedTask;
    }
}

/// <summary>For tests that build the services by hand and don't care about alerts.</summary>
public class NoAlerts : IStaffAlerts
{
    public Task SupportRequestReceivedAsync(PhilisaAbantuBethu.Models.SupportRequest request, CancellationToken ct = default) => Task.CompletedTask;
}

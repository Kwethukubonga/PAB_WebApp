using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Data.Protection;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>What someone writes about their situation is encrypted in the database (A5).</summary>
public class SensitiveTextTests
{
    // An ephemeral provider makes its own key, so each one here stands for a separate server and
    // nothing depends on what keys this machine happens to have.
    private static SensitiveText Build() =>
        new(new EphemeralDataProtectionProvider(), NullLogger<SensitiveText>.Instance);

    [Fact]
    public void Protected_text_does_not_contain_what_was_written()
    {
        var sensitive = Build();
        const string situation = "He came home drunk again and the children are frightened.";

        var stored = sensitive.Protect(situation);

        Assert.DoesNotContain("drunk", stored);
        Assert.DoesNotContain("children", stored);
        Assert.StartsWith("enc:v1:", stored);
    }

    [Fact]
    public void Protected_text_reads_back_exactly()
    {
        var sensitive = Build();
        const string situation = "Ndifuna indawo ekhuselekileyo namhlanje ebusuku.";

        Assert.Equal(situation, sensitive.Unprotect(sensitive.Protect(situation)));
    }

    [Fact]
    public void The_same_text_encrypts_differently_each_time()
    {
        var sensitive = Build();

        Assert.NotEqual(sensitive.Protect("same words"), sensitive.Protect("same words"));
    }

    [Fact]
    public void Text_saved_before_this_was_added_still_opens()
    {
        var sensitive = Build();

        // A row written when the column held plain text has no marker on it.
        Assert.Equal("an older row", sensitive.Unprotect("an older row"));
    }

    [Fact]
    public void An_empty_situation_stays_empty()
    {
        var sensitive = Build();

        Assert.Equal("", sensitive.Protect(""));
        Assert.Equal("", sensitive.Unprotect(""));
    }

    [Fact]
    public void Text_encrypted_with_another_key_does_not_crash_the_page()
    {
        var stored = Build().Protect("written on another server");

        // A different server, so a different key: the text is gone, but nothing throws.
        Assert.Equal("", Build().Unprotect(stored));
    }
}

/// <summary>Closed requests and old messages are deleted once past their retention period (A5).</summary>
public class RetentionServiceTests
{
    // ExecuteDelete is real SQL, so these run against SQLite rather than the in-memory provider.
    // The connection is handed over and kept open, which is what keeps the database alive.
    private static AppDbContext NewDb()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        return db;
    }

    private static RetentionService Build(AppDbContext db, int months = 12) =>
        new(db, Options.Create(new RetentionOptions { ClosedRequestMonths = months, ContactMessageMonths = months }),
            NullLogger<RetentionService>.Instance);

    private static SupportRequest Request(string status, DateTime submitted, DateTime? closed = null) => new()
    {
        FirstName = "Test", Surname = "Person", Phone = "082 123 4567", Email = "", Area = "Steenberg",
        ContactMethod = "phone", SupportType = "Food Assistance", Situation = "A situation.",
        Urgent = "no", Extra = "", Declaration = true, Reference = $"PAB-{Guid.NewGuid():N}"[..10],
        SubmittedAt = submitted, Status = status, StatusUpdatedAt = closed
    };

    [Fact]
    public async Task A_request_closed_over_a_year_ago_is_deleted()
    {
        var now = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        using var db = NewDb();
        db.SupportRequests.Add(Request(SupportRequestStatus.Closed, now.AddYears(-2), now.AddMonths(-13)));
        await db.SaveChangesAsync();

        var result = await Build(db).RunAsync(now);

        Assert.Equal(1, result.ClosedRequests);
        Assert.Empty(db.SupportRequests);
    }

    [Fact]
    public async Task A_request_closed_last_month_is_kept()
    {
        var now = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        using var db = NewDb();
        db.SupportRequests.Add(Request(SupportRequestStatus.Closed, now.AddMonths(-2), now.AddMonths(-1)));
        await db.SaveChangesAsync();

        var result = await Build(db).RunAsync(now);

        Assert.Equal(0, result.ClosedRequests);
        Assert.Single(db.SupportRequests);
    }

    [Fact]
    public async Task An_old_request_nobody_closed_is_never_deleted()
    {
        var now = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        using var db = NewDb();
        db.SupportRequests.Add(Request(SupportRequestStatus.New, now.AddYears(-3)));
        db.SupportRequests.Add(Request(SupportRequestStatus.InProgress, now.AddYears(-3)));
        await db.SaveChangesAsync();

        var result = await Build(db).RunAsync(now);

        Assert.Equal(0, result.ClosedRequests);
        Assert.Equal(2, db.SupportRequests.Count());
    }

    [Fact]
    public async Task Old_contact_messages_are_deleted()
    {
        var now = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        using var db = NewDb();
        db.ContactInquiries.Add(new ContactInquiry
        {
            Name = "Old", Email = "old@example.org", Phone = "", Subject = "Media",
            Message = "A while ago.", SubmittedAt = now.AddMonths(-18)
        });
        db.ContactInquiries.Add(new ContactInquiry
        {
            Name = "Recent", Email = "new@example.org", Phone = "", Subject = "Media",
            Message = "This week.", SubmittedAt = now.AddDays(-3)
        });
        await db.SaveChangesAsync();

        var result = await Build(db).RunAsync(now);

        Assert.Equal(1, result.ContactMessages);
        Assert.Equal("Recent", db.ContactInquiries.Single().Name);
    }

    [Fact]
    public async Task Setting_the_period_to_zero_turns_the_cleanup_off()
    {
        var now = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);
        using var db = NewDb();
        db.SupportRequests.Add(Request(SupportRequestStatus.Closed, now.AddYears(-5), now.AddYears(-5)));
        await db.SaveChangesAsync();

        var result = await Build(db, months: 0).RunAsync(now);

        Assert.Equal(0, result.ClosedRequests);
        Assert.Single(db.SupportRequests);
    }
}

public class DatabaseProviderTests
{
    [Theory]
    [InlineData("Data Source=app.db", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("Host=localhost;Database=pab;Username=pab;Password=secret", true)]
    [InlineData("postgresql://pab:secret@containers.railway.app:5432/railway", true)]
    public void The_connection_string_decides_the_provider(string? connectionString, bool postgres)
    {
        Assert.Equal(postgres, DatabaseProvider.IsPostgres(connectionString));
    }

    [Fact]
    public void A_postgres_url_becomes_a_connection_string_npgsql_understands()
    {
        var result = DatabaseProvider.ToNpgsql("postgresql://pab:s3cr3t@db.railway.internal:5433/railway");

        Assert.Contains("Host=db.railway.internal", result);
        Assert.Contains("Port=5433", result);
        Assert.Contains("Database=railway", result);
        Assert.Contains("Username=pab", result);
        Assert.Contains("Password=s3cr3t", result);
        Assert.Contains("SSL Mode=Require", result);
    }

    [Fact]
    public void A_url_without_a_port_falls_back_to_the_postgres_default()
    {
        Assert.Contains("Port=5432", DatabaseProvider.ToNpgsql("postgresql://pab:pw@host.example/railway"));
    }

    [Fact]
    public void A_keyword_connection_string_is_left_alone()
    {
        const string given = "Host=localhost;Database=pab";

        Assert.Equal(given, DatabaseProvider.ToNpgsql(given));
    }
}

public class DevelopmentSeedTests
{
    [Theory]
    [InlineData(0, SupportRequestStatus.New)]
    [InlineData(2, SupportRequestStatus.New)]
    [InlineData(3, SupportRequestStatus.InProgress)]
    [InlineData(30, SupportRequestStatus.InProgress)]
    [InlineData(31, SupportRequestStatus.Closed)]
    public void The_status_follows_from_how_old_the_request_is(int daysAgo, string status)
    {
        // The seed works its statuses out rather than listing them, so it can't contradict itself.
        Assert.Equal(status, DevelopmentSeed.StatusFor(daysAgo));
    }
}

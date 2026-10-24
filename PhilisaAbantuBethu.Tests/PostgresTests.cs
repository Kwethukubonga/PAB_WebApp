using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Npgsql;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Data.Protection;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// Runs only when PAB_TEST_POSTGRES names a Postgres server (a connection string to its
/// administrative database). CI starts one; on a laptop: docker run -e POSTGRES_PASSWORD=x -p 5433:5432 postgres
/// then PAB_TEST_POSTGRES="Host=localhost;Port=5433;Username=postgres;Password=x".
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public const string Variable = "PAB_TEST_POSTGRES";

    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
        {
            Skip = $"Set {Variable} to a Postgres server's connection string to run this.";
        }
    }
}

/// <summary>A database made for one test and dropped after it.</summary>
public sealed class PostgresTestDatabase : IDisposable
{
    private readonly string _admin = Environment.GetEnvironmentVariable(PostgresFactAttribute.Variable)!;
    private readonly string _name = $"pab_test_{Guid.NewGuid():N}";

    public PostgresTestDatabase()
    {
        Run(_admin, $"CREATE DATABASE \"{_name}\"");
        ConnectionString = new NpgsqlConnectionStringBuilder(_admin) { Database = _name }.ConnectionString;
    }

    public string ConnectionString { get; }

    public string? Scalar(string sql)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        return command.ExecuteScalar()?.ToString();
    }

    private static void Run(string connectionString, string sql)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        NpgsqlConnection.ClearAllPools();
        Run(_admin, $"DROP DATABASE IF EXISTS \"{_name}\" WITH (FORCE)");
    }
}

/// <summary>A throwaway certificate of the kind tools/data-protection/New-KeyCertificate.ps1 makes.</summary>
public static class TestCertificate
{
    public static (string Base64, string Password) Create()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=PAB test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        var password = Guid.NewGuid().ToString("N");
        return (Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, password)), password);
    }
}

public class DataProtectionSetupTests
{
    private static IConfiguration Settings(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value)).Build();

    private static void Add(IConfiguration config, string environment, bool keysInDatabase) =>
        new ServiceCollection().AddSiteDataProtection(config, new FakeEnvironment(environment), keysInDatabase);

    [Fact]
    public void A_live_site_with_its_keys_in_the_database_refuses_to_start_without_the_certificate()
    {
        // Better no new version than one that keeps the keys unprotected next to the text they open.
        var error = Assert.Throws<InvalidOperationException>(() => Add(Settings(), Environments.Production, keysInDatabase: true));
        Assert.Contains("DataProtection:Certificate", error.Message);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Developing_and_testing_do_not_need_the_certificate(string environment) =>
        Add(Settings(), environment, keysInDatabase: true);

    [Fact]
    public void Keys_kept_in_files_need_no_certificate()
    {
        Add(Settings(), Environments.Production, keysInDatabase: false);
    }

    [Fact]
    public void A_certificate_given_as_text_is_opened_with_its_private_key()
    {
        var (base64, password) = TestCertificate.Create();

        using var certificate = DataProtectionSetup.LoadCertificate(Settings(
            ("DataProtection:Certificate", base64), ("DataProtection:CertificatePassword", password)));

        Assert.NotNull(certificate);
        Assert.True(certificate!.HasPrivateKey);
    }

    [Fact]
    public void A_certificate_given_as_a_file_is_opened()
    {
        var (base64, password) = TestCertificate.Create();
        var path = Path.Combine(Path.GetTempPath(), $"pab-cert-{Guid.NewGuid():N}.pfx");
        File.WriteAllBytes(path, Convert.FromBase64String(base64));
        try
        {
            using var certificate = DataProtectionSetup.LoadCertificate(Settings(
                ("DataProtection:CertificatePath", path), ("DataProtection:CertificatePassword", password)));

            Assert.True(certificate!.HasPrivateKey);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void A_wrong_password_is_reported_without_repeating_it()
    {
        var (base64, _) = TestCertificate.Create();
        const string wrong = "not-the-password-4421";

        var error = Assert.Throws<InvalidOperationException>(() => DataProtectionSetup.LoadCertificate(Settings(
            ("DataProtection:Certificate", base64), ("DataProtection:CertificatePassword", wrong))));

        Assert.Contains("CertificatePassword", error.Message);
        Assert.DoesNotContain(wrong, error.ToString());
    }

    [Fact]
    public void Text_that_is_not_base64_is_reported()
    {
        var error = Assert.Throws<InvalidOperationException>(() => DataProtectionSetup.LoadCertificate(Settings(
            ("DataProtection:Certificate", "this is not base64!"))));

        Assert.Contains("base64", error.Message);
    }

    [Fact]
    public void No_certificate_settings_means_no_certificate() =>
        Assert.Null(DataProtectionSetup.LoadCertificate(Settings()));

    [Fact]
    public void The_database_address_closes_idle_connections_and_keeps_a_small_pool()
    {
        // A sleeping managed database drops connections nobody has used; the pool must not hand
        // those out dead.
        var connection = DatabaseProvider.ToNpgsql("postgresql://pab:pw@ep-example.eu-central-1.aws.neon.tech/neondb?sslmode=require");

        Assert.Contains("Connection Idle Lifetime=30", connection);
        Assert.Contains("Maximum Pool Size=10", connection);
    }

    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "PhilisaAbantuBethu";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

/// <summary>
/// The Postgres side, against a real server. Each test gets its own database. See
/// <see cref="PostgresFactAttribute"/> for how to run these.
/// </summary>
public class PostgresTests
{
    private static readonly object SupportBody = new
    {
        firstName = "Nomsa", surname = "Mkoka", phone = "082 123 4567", area = "Lavender Hill",
        contactMethod = "phone", supportType = "Emergency Shelter",
        situation = "I need somewhere safe for tonight.", urgent = "yes", declaration = true
    };

    private static SiteFactory SiteOn(PostgresTestDatabase database, (string Base64, string Password) certificate) => new()
    {
        ConnectionString = database.ConnectionString,
        DataProtectionCertificate = certificate.Base64,
        DataProtectionCertificatePassword = certificate.Password
    };

    private static async Task SaveARequest(SiteFactory site)
    {
        var response = await site.CreateBrowser().PostAsJsonAsync("/api/support-requests", SupportBody);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [PostgresFact]
    public void The_migrations_build_the_whole_schema_on_an_empty_database()
    {
        using var database = new PostgresTestDatabase();
        using var site = SiteOn(database, TestCertificate.Create());

        site.WithDb(db =>
        {
            // The site migrated the database when it started. The snapshot must also match the model,
            // or the next schema change would silently carry this one's mistakes with it.
            Assert.IsType<PostgresAppDbContext>(db);
            Assert.False(db.Database.HasPendingModelChanges());
            return 0;
        });

        foreach (var table in new[] { "SupportRequests", "ContactInquiries", "DataProtectionKeys" })
        {
            Assert.Equal("1", database.Scalar($"SELECT count(*) FROM information_schema.tables WHERE table_name = '{table}'"));
        }
        Assert.Equal("1", database.Scalar("SELECT count(*) FROM pg_indexes WHERE indexname = 'IX_SupportRequests_Reference'"));
    }

    [PostgresFact]
    public async Task A_request_is_stored_encrypted_and_read_back_in_the_clear()
    {
        using var database = new PostgresTestDatabase();
        using var site = SiteOn(database, TestCertificate.Create());

        await SaveARequest(site);

        var stored = database.Scalar("SELECT \"Situation\" FROM \"SupportRequests\"")!;
        Assert.StartsWith(SensitiveText.Marker, stored);
        Assert.DoesNotContain("somewhere safe", stored);

        var read = site.WithDb(db => db.SupportRequests.Single().Situation);
        Assert.Equal("I need somewhere safe for tonight.", read);
    }

    [PostgresFact]
    public async Task The_keys_are_in_the_database_and_locked_with_the_certificate()
    {
        using var database = new PostgresTestDatabase();
        using var site = SiteOn(database, TestCertificate.Create());

        await SaveARequest(site);

        Assert.NotEqual("0", database.Scalar("SELECT count(*) FROM \"DataProtectionKeys\""));
        var xml = database.Scalar("SELECT \"Xml\" FROM \"DataProtectionKeys\" ORDER BY \"Id\" LIMIT 1")!;
        // Locked: the key material is wrapped in an encrypted element, not written as plain text.
        Assert.Contains("EncryptedData", xml);
        Assert.DoesNotContain("<masterKey", xml);
    }

    // What the site's own keys make of the stored text, without going through a query.
    private static string Open(SiteFactory site, PostgresTestDatabase database) =>
        site.Services.GetRequiredService<ISensitiveText>()
            .Unprotect(database.Scalar("SELECT \"Situation\" FROM \"SupportRequests\"")!);

    [PostgresFact]
    public async Task The_decryption_check_counts_what_opens_and_what_does_not()
    {
        using var database = new PostgresTestDatabase();
        var certificate = TestCertificate.Create();

        using (var first = SiteOn(database, certificate))
        {
            await SaveARequest(first);
        }

        using (var same = SiteOn(database, certificate))
        {
            var output = new StringWriter();
            Assert.Equal(0, await DecryptionCheck.RunAsync(same.Services, output));
            Assert.Contains("Support requests: 1. Readable: 1. Unreadable: 0.", output.ToString());
            Assert.DoesNotContain("safe for tonight", output.ToString());
        }

        using (var wrong = SiteOn(database, TestCertificate.Create()))
        {
            var output = new StringWriter();
            Assert.Equal(1, await DecryptionCheck.RunAsync(wrong.Services, output));
            Assert.Contains("Unreadable: 1.", output.ToString());
            Assert.Contains("FAILED", output.ToString());
        }
    }

    [PostgresFact]
    public async Task After_a_restart_a_saved_request_still_opens()
    {
        using var database = new PostgresTestDatabase();
        var certificate = TestCertificate.Create();

        using (var first = SiteOn(database, certificate))
        {
            await SaveARequest(first);
        }

        // A new process on the same database and certificate: nothing is on its disk.
        using var second = SiteOn(database, certificate);
        Assert.Equal("I need somewhere safe for tonight.", Open(second, database));
        Assert.Equal("1", database.Scalar("SELECT count(*) FROM \"DataProtectionKeys\""));
    }

    [PostgresFact]
    public async Task Without_the_certificate_a_copy_of_the_database_cannot_be_read()
    {
        using var database = new PostgresTestDatabase();

        using (var first = SiteOn(database, TestCertificate.Create()))
        {
            await SaveARequest(first);
        }

        // Someone with the database but not the certificate. The keys will not open, so the text
        // stays locked.
        using var thief = SiteOn(database, TestCertificate.Create());
        Assert.Equal("", Open(thief, database));
        Assert.Equal("", thief.WithDb(db => db.SupportRequests.Single().Situation));
        Assert.StartsWith(SensitiveText.Marker, database.Scalar("SELECT \"Situation\" FROM \"SupportRequests\"")!);
    }
}

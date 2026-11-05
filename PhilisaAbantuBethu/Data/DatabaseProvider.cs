namespace PhilisaAbantuBethu.Data;

/// <summary>
/// Which database the connection string points at. A single SQLite file inside the container was
/// the marker's Hosting point: it is lost on every redeploy and can't be backed up. The site now
/// runs on Postgres wherever one is configured, and falls back to SQLite for local work, so there
/// is nothing to change in code when PAB's managed database is added (C1).
/// </summary>
public static class DatabaseProvider
{
    /// <summary>
    /// True for a Postgres connection string, in either form hosting providers hand out: the
    /// keyword form (<c>Host=…;Database=…</c>) or the URL form (<c>postgresql://…</c>).
    /// </summary>
    public static bool IsPostgres(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return false;

        return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);
    }

    public static string Name(string? connectionString) => IsPostgres(connectionString) ? "Postgres" : "SQLite";

    /// <summary>
    /// Npgsql wants <c>Host=…;Port=…;Database=…;Username=…;Password=…</c>, but Render, Neon and
    /// Supabase all hand out a <c>postgresql://</c> URL. This turns one into the other and leaves
    /// a keyword string alone, so DATABASE_URL can be used as given.
    /// </summary>
    public static string ToNpgsql(string connectionString)
    {
        if (!connectionString.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)
            || !connectionString.Contains("://", StringComparison.Ordinal))
        {
            return connectionString;
        }

        var url = new Uri(connectionString);
        var credentials = url.UserInfo.Split(':', 2);
        var parts = new List<string>
        {
            $"Host={url.Host}",
            $"Port={(url.IsDefaultPort ? 5432 : url.Port)}",
            $"Database={url.AbsolutePath.Trim('/')}",
            $"Username={Uri.UnescapeDataString(credentials[0])}"
        };
        if (credentials.Length > 1) parts.Add($"Password={Uri.UnescapeDataString(credentials[1])}");

        // Managed Postgres is TLS-only, and its certificate is the provider's own.
        parts.Add("SSL Mode=Require");
        parts.Add("Trust Server Certificate=true");

        // A managed database that sleeps drops connections nobody has used for a few minutes. If
        // the pool kept one of those it would hand it out dead, so unused connections are closed
        // after 30 seconds. A small pool is plenty for this site and keeps well inside the
        // connection limit of a free plan.
        parts.Add("Connection Idle Lifetime=30");
        parts.Add("Maximum Pool Size=10");
        return string.Join(';', parts);
    }
}

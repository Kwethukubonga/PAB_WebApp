using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data.Protection;

namespace PhilisaAbantuBethu.Data;

/// <summary>
/// The same model as <see cref="AppDbContext"/>, used when the connection string points at Postgres.
/// It exists for two reasons.
///
/// The migrations in Migrations/ were generated for SQLite and use its column types, so Postgres
/// gets its own set in Migrations/Postgres. Migrations belong to a context type, so a separate
/// type is what keeps the two sets from being mixed up.
///
/// It also holds the table the data protection keys are stored in. A host with no disk that
/// survives a restart (Render's free plan) can't keep them in files, and a restart that lost them
/// would make every saved support request unreadable.
/// </summary>
public class PostgresAppDbContext : AppDbContext, IDataProtectionKeyContext
{
    public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options, ISensitiveText sensitive)
        : base(options, sensitive) { }

    /// <summary>For the EF design-time tools, which build the model without the app's services.</summary>
    internal PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options) : base(options, null) { }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}

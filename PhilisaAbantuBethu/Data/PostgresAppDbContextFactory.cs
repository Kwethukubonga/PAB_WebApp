using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PhilisaAbantuBethu.Data;

/// <summary>
/// Lets <c>dotnet ef migrations add --context PostgresAppDbContext</c> build the context without a
/// database. Generating a migration only reads the model, so the address below is never connected
/// to. The app itself never uses this class.
/// </summary>
public class PostgresAppDbContextFactory : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PostgresAppDbContext>()
            .UseNpgsql("Host=localhost;Database=pab-design-time")
            .Options;
        return new PostgresAppDbContext(options);
    }
}

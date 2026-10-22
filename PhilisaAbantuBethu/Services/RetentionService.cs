using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Services;

/// <summary>How long PAB keeps what people send. Set under "Retention" in appsettings.</summary>
public class RetentionOptions
{
    public const string Section = "Retention";

    /// <summary>Months a closed support request is kept after it was closed. 0 turns the cleanup off.</summary>
    public int ClosedRequestMonths { get; set; } = 12;

    /// <summary>Months a contact message is kept after it was sent. 0 turns the cleanup off.</summary>
    public int ContactMessageMonths { get; set; } = 12;

    /// <summary>How often the cleanup runs.</summary>
    public int RunEveryHours { get; set; } = 24;
}

public interface IRetentionService
{
    /// <summary>Deletes what is past its retention period and returns how many rows went.</summary>
    Task<RetentionResult> RunAsync(DateTime now, CancellationToken ct = default);
}

public record RetentionResult(int ClosedRequests, int ContactMessages);

/// <summary>
/// POPIA says personal information is kept only as long as it is needed. Closed support requests
/// and contact messages are deleted once they are past the period on the Privacy page (A5).
/// Requests that are still new or in progress are never touched, however old they are.
/// </summary>
public class RetentionService : IRetentionService
{
    private readonly AppDbContext _db;
    private readonly RetentionOptions _options;
    private readonly ILogger<RetentionService> _log;

    public RetentionService(AppDbContext db, IOptions<RetentionOptions> options, ILogger<RetentionService> log)
    {
        _db = db;
        _options = options.Value;
        _log = log;
    }

    public async Task<RetentionResult> RunAsync(DateTime now, CancellationToken ct = default)
    {
        var requests = 0;
        if (_options.ClosedRequestMonths > 0)
        {
            var cutoff = now.AddMonths(-_options.ClosedRequestMonths);
            // Closed long ago. StatusUpdatedAt says when it was closed; fall back to when it came in.
            requests = await _db.SupportRequests
                .Where(r => r.Status == SupportRequestStatus.Closed
                    && (r.StatusUpdatedAt ?? r.SubmittedAt) < cutoff)
                .ExecuteDeleteAsync(ct);
        }

        var messages = 0;
        if (_options.ContactMessageMonths > 0)
        {
            var cutoff = now.AddMonths(-_options.ContactMessageMonths);
            messages = await _db.ContactInquiries
                .Where(c => c.SubmittedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }

        if (requests > 0 || messages > 0)
        {
            _log.LogInformation(
                "Retention removed {Requests} closed support requests and {Messages} contact messages",
                requests, messages);
        }
        return new RetentionResult(requests, messages);
    }
}

/// <summary>Runs <see cref="RetentionService"/> on a timer for as long as the site is up.</summary>
public class RetentionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly RetentionOptions _options;
    private readonly ILogger<RetentionWorker> _log;

    public RetentionWorker(IServiceScopeFactory scopes, IOptions<RetentionOptions> options, ILogger<RetentionWorker> log)
    {
        _scopes = scopes;
        _options = options.Value;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var every = TimeSpan.FromHours(Math.Max(1, _options.RunEveryHours));
        using var timer = new PeriodicTimer(every);

        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var retention = scope.ServiceProvider.GetRequiredService<IRetentionService>();
                await retention.RunAsync(DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                // A failed cleanup must never take the site down with it.
                _log.LogError(ex, "The retention cleanup did not finish; it will try again later");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

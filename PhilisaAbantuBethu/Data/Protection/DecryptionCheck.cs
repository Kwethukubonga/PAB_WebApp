using Microsoft.EntityFrameworkCore;

namespace PhilisaAbantuBethu.Data.Protection;

/// <summary>
/// Opens every saved support request and says how many could be read. Run after restoring a backup
/// (<c>dotnet run --project PhilisaAbantuBethu -- --check-decryption</c>) to prove the backup and the
/// certificate together still open what people wrote. It prints counts only, never the text.
/// </summary>
public static class DecryptionCheck
{
    public const string Argument = "--check-decryption";

    /// <summary>0 when every request opened, 1 when any could not be read.</summary>
    public static async Task<int> RunAsync(IServiceProvider services, TextWriter output)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The situation is required, so an empty one after decrypting means the keys didn't open it.
        var situations = await db.SupportRequests.AsNoTracking().Select(r => r.Situation).ToListAsync();
        var unreadable = situations.Count(situation => string.IsNullOrEmpty(situation));
        var contacts = await db.ContactInquiries.CountAsync();

        output.WriteLine($"Support requests: {situations.Count}. Readable: {situations.Count - unreadable}. Unreadable: {unreadable}.");
        output.WriteLine($"Contact messages: {contacts}.");
        output.WriteLine(unreadable == 0 ? "OK: everything that is encrypted opens." : "FAILED: some text could not be decrypted with the current keys.");

        return unreadable == 0 ? 0 : 1;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

public class ReferenceGeneratorTests
{
    [Fact]
    public void References_LookLikePabThenSixEasyToReadCharacters()
    {
        var reference = new ReferenceGenerator().Next();

        Assert.Matches("^PAB-[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{6}$", reference);
    }

    // Random references can repeat now and then, which the database's unique index and the retry
    // below deal with. This only catches a broken generator: two repeats in a thousand would
    // happen about once in six million runs.
    [Fact]
    public void AThousandReferences_AreAlmostAllDifferent()
    {
        var generator = new ReferenceGenerator();

        var references = Enumerable.Range(0, 1_000).Select(_ => generator.Next()).ToList();

        Assert.True(references.Distinct().Count() >= 999);
    }

    /// <summary>Hands out the given references in order, to force a clash.</summary>
    private class ScriptedGenerator : IReferenceGenerator
    {
        private readonly Queue<string> _next;
        public ScriptedGenerator(params string[] references) => _next = new Queue<string>(references);
        public string Next() => _next.Dequeue();
    }

    [Fact]
    public async Task Submit_DrawsAgain_WhenTheReferenceIsAlreadyTaken()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new AppDbContext(options);
        db.SupportRequests.Add(new SupportRequest { Reference = "PAB-AAAAAA", SubmittedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new SupportRequestService(db, new ScriptedGenerator("PAB-AAAAAA", "PAB-BBBBBB"), NullLogger<SupportRequestService>.Instance);
        var reference = await service.SubmitAsync(new SupportRequestInput(
            "Nomsa", "Dlamini", "071 123 4567", "", "Lavender Hill", "phone",
            "Food Assistance", "Need groceries", "no", "", true));

        Assert.Equal("PAB-BBBBBB", reference);
        Assert.Equal(2, db.SupportRequests.Select(r => r.Reference).Distinct().Count());
    }
}

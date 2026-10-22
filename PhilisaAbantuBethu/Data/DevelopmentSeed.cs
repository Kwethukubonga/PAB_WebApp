using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Data;

/// <summary>
/// Example support requests and messages, so a developer or a demo opens the staff area with
/// something in every status instead of an empty list (W9).
///
/// Dates count backwards from whenever it runs, not fixed days, so the list always has something
/// that arrived today and something old enough to be closed. The status follows from how old the
/// row is rather than being typed in, so the seed can't contradict itself.
///
/// It runs in development only, and only when the table is empty, so it can never touch what
/// somebody really sent.
/// </summary>
public static class DevelopmentSeed
{
    private record Example(int DaysAgo, string FirstName, string Surname, string Area, string SupportType,
        string ContactMethod, bool Urgent, string Situation);

    private static readonly Example[] Examples =
    {
        new(0, "Nomsa", "Mkoka", "Lavender Hill", "Emergency Shelter", "phone", true,
            "I had to leave home last night with my two children and we have nowhere to sleep tonight."),
        new(1, "Thandi", "Nkosi", "Steenberg", "Food Assistance", "whatsapp", false,
            "I lost my job last month and the food parcels would help until my grant comes through."),
        new(4, "Gail", "Peters", "Retreat", "Counselling", "phone", true,
            "My daughter is struggling after what happened at school and we would like to speak to someone."),
        new(11, "Lindsay", "Adams", "Grassy Park", "Legal Assistance", "email", false,
            "I need help understanding a protection order and what happens at the court date."),
        new(26, "Olivia", "Maart", "Seawinds", "Skills Training", "visit", false,
            "I would like to join the sewing workshops and hear about the savings group."),
        new(70, "Zimvo", "Katsala", "Lotus River", "Youth Support", "whatsapp", false,
            "My son is 16 and out of school. I want to know what the youth programme offers.")
    };

    public static async Task RunAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.SupportRequests.AnyAsync() || await db.ContactInquiries.AnyAsync()) return;

        var references = services.GetRequiredService<IReferenceGenerator>();
        var now = DateTime.UtcNow;

        foreach (var example in Examples)
        {
            var submitted = now.AddDays(-example.DaysAgo).AddHours(-2);
            var status = StatusFor(example.DaysAgo);

            db.SupportRequests.Add(new SupportRequest
            {
                FirstName = example.FirstName,
                Surname = example.Surname,
                Phone = "082 123 4567",
                Email = example.ContactMethod == "email"
                    ? $"{example.FirstName.ToLowerInvariant()}@example.org"
                    : "",
                Area = example.Area,
                ContactMethod = example.ContactMethod,
                SupportType = example.SupportType,
                Situation = example.Situation,
                Urgent = example.Urgent ? "yes" : "no",
                Extra = "",
                Declaration = true,
                Reference = references.Next(),
                SubmittedAt = submitted,
                Status = status,
                // Only a request someone has picked up has these, and never before it came in.
                StatusUpdatedAt = status == SupportRequestStatus.New ? null : submitted.AddDays(1),
                HandledBy = status == SupportRequestStatus.New ? null : "intake@philisaabafazi.org",
                StaffNotes = status == SupportRequestStatus.Closed
                    ? "Referred to the social work team and followed up by phone."
                    : null
            });
        }

        db.ContactInquiries.Add(new ContactInquiry
        {
            Name = "Brandon Samuels",
            Email = "brandon@example.org",
            Phone = "083 555 0101",
            Subject = "Volunteering",
            Message = "I am a final-year social work student and would like to volunteer on Saturdays.",
            SubmittedAt = now.AddDays(-2)
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// How far along a request of this age would be: something from this week is still new, the
    /// last month's is being worked on, and anything older has been dealt with.
    /// </summary>
    internal static string StatusFor(int daysAgo) => daysAgo switch
    {
        <= 2 => SupportRequestStatus.New,
        <= 30 => SupportRequestStatus.InProgress,
        _ => SupportRequestStatus.Closed
    };
}

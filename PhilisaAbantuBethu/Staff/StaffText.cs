using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Staff;

/// <summary>Labels and times shown on the staff pages, in the visitor's language.</summary>
public static class StaffText
{
    // South Africa has no daylight saving, so SAST is always two hours ahead of UTC.
    private static readonly TimeSpan Sast = TimeSpan.FromHours(2);

    public static string StatusLabel(SiteStrings s, string status) => status switch
    {
        SupportRequestStatus.InProgress => s["StaffText.InProgress"],
        SupportRequestStatus.Closed => s["StaffText.Closed"],
        _ => s["StaffText.New"]
    };

    public static string ContactMethodLabel(SiteStrings s, string method) => method switch
    {
        "whatsapp" => s.Form.ContactWhatsApp,
        "email" => s.Form.ContactEmail,
        "visit" => s.Form.ContactVisit,
        _ => s.Form.ContactPhone
    };

    /// <summary>Stored times are UTC; staff read them in Cape Town time, e.g. 20 Oct 2026, 14:05.</summary>
    public static string LocalTime(DateTime utc) =>
        (DateTime.SpecifyKind(utc, DateTimeKind.Utc) + Sast).ToString("d MMM yyyy, HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>"5 minutes ago", "3 hours ago", "2 days ago".</summary>
    public static string Ago(SiteStrings s, DateTime utc, DateTime nowUtc)
    {
        var span = nowUtc - DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        if (span < TimeSpan.FromMinutes(1)) return s["StaffText.JustNow"];
        if (span < TimeSpan.FromHours(1))
        {
            var m = (int)span.TotalMinutes;
            return s.Format("StaffText.MinAgo", m);
        }
        if (span < TimeSpan.FromDays(1))
        {
            var h = (int)span.TotalHours;
            // One and many are separate entries because English and Afrikaans change the word.
            return s.Format(h == 1 ? "StaffText.HourAgo" : "StaffText.HoursAgo", h);
        }
        var d = (int)span.TotalDays;
        return s.Format(d == 1 ? "StaffText.DayAgo" : "StaffText.DaysAgo", d);
    }

    /// <summary>
    /// The pill class for a status. One colour per status, from the shared tokens, so a request
    /// looks the same in the list, on its own page and in the Android app (W12, E1).
    /// </summary>
    public static string StatusPill(string status) => status switch
    {
        SupportRequestStatus.New => "pill-new",
        SupportRequestStatus.InProgress => "pill-working",
        _ => "pill-closed"
    };
}

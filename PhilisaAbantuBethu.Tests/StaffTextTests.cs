using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Tests;

public class StaffTextTests
{
    private static readonly DateTime Now = new(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("en", 0, "just now")]
    [InlineData("en", 5, "5 min ago")]
    [InlineData("en", 60, "1 hour ago")]
    [InlineData("en", 180, "3 hours ago")]
    [InlineData("en", 1440, "1 day ago")]
    [InlineData("en", 2880, "2 days ago")]
    [InlineData("xh", 0, "ngoku nje")]
    [InlineData("xh", 5, "imizuzu engu-5 edlulileyo")]
    [InlineData("xh", 60, "iiyure ezingu-1 ezidlulileyo")]
    [InlineData("xh", 180, "iiyure ezingu-3 ezidlulileyo")]
    [InlineData("xh", 1440, "iintsuku ezingu-1 ezidlulileyo")]
    [InlineData("xh", 2880, "iintsuku ezingu-2 ezidlulileyo")]
    [InlineData("af", 0, "net nou")]
    [InlineData("af", 5, "5 min gelede")]
    [InlineData("af", 60, "1 uur gelede")]
    [InlineData("af", 180, "3 uur gelede")]
    [InlineData("af", 1440, "1 dag gelede")]
    [InlineData("af", 2880, "2 dae gelede")]
    public void Ago_SaysHowLongItsBeen_InEachLanguage(string lang, int minutesAgo, string expected) =>
        Assert.Equal(expected, StaffText.Ago(SiteStrings.For(lang), Now.AddMinutes(-minutesAgo), Now));

    [Theory]
    [InlineData("en", "New", "In progress", "Closed")]
    [InlineData("xh", "Entsha", "Iyaqhubeka", "Ivaliwe")]
    [InlineData("af", "Nuut", "Aan die gang", "Gesluit")]
    public void StatusLabel_NamesEachStatus_InEachLanguage(string lang, string isNew, string inProgress, string closed)
    {
        var s = SiteStrings.For(lang);

        Assert.Equal(isNew, StaffText.StatusLabel(s, SupportRequestStatus.New));
        Assert.Equal(inProgress, StaffText.StatusLabel(s, SupportRequestStatus.InProgress));
        Assert.Equal(closed, StaffText.StatusLabel(s, SupportRequestStatus.Closed));
    }
}

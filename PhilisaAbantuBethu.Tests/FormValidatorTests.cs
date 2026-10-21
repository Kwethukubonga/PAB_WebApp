using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class FormValidatorTests
{
    private static SupportRequestInput ValidRequest() => new(
        "Nomsa", "Dlamini", "071 123 4567", "nomsa@example.com", "Lavender Hill", "phone",
        "Food Assistance", "Need help with groceries this month.", "no", "", true);

    [Fact]
    public void SupportRequest_PassesWhenComplete() =>
        Assert.Empty(SupportRequestValidator.Validate(ValidRequest(), SiteStrings.En));

    [Fact]
    public void SupportRequest_NamesEveryMissingField()
    {
        var errors = SupportRequestValidator.Validate(
            new SupportRequestInput(null, " ", null, null, "", null, null, "", null, null, false), SiteStrings.En);

        Assert.Equal(
            new[] { "Area", "ContactMethod", "Declaration", "FirstName", "Phone", "Situation", "SupportType", "Surname", "Urgent" },
            errors.Keys.OrderBy(k => k));
    }

    [Fact]
    public void SupportRequest_RefusesAnInventedSupportType() =>
        Assert.Contains("SupportType", SupportRequestValidator.Validate(ValidRequest() with { SupportType = "<script>" }, SiteStrings.En).Keys);

    [Fact]
    public void SupportRequest_AcceptsASupportTypeInAnyLanguage()
    {
        // The form sends the label in the visitor's language.
        Assert.DoesNotContain("SupportType", SupportRequestValidator.Validate(ValidRequest() with { SupportType = SiteStrings.Xh.SupportTypes[0] }, SiteStrings.En).Keys);
        Assert.DoesNotContain("SupportType", SupportRequestValidator.Validate(ValidRequest() with { SupportType = SiteStrings.Af.SupportTypes[0] }, SiteStrings.En).Keys);
    }

    [Fact]
    public void SupportRequest_NeedsAnEmail_WhenEmailIsTheChosenWayToReply()
    {
        var errors = SupportRequestValidator.Validate(ValidRequest() with { ContactMethod = "email", Email = "" }, SiteStrings.En);
        Assert.Contains("Email", errors.Keys);
    }

    [Theory]
    [InlineData("nomsa@home")]
    [InlineData("not an email")]
    [InlineData("@example.com")]
    public void SupportRequest_RefusesABadEmail(string email) =>
        Assert.Contains("Email", SupportRequestValidator.Validate(ValidRequest() with { Email = email }, SiteStrings.En).Keys);

    [Fact]
    public void SupportRequest_RefusesTooMuchText()
    {
        var errors = SupportRequestValidator.Validate(
            ValidRequest() with { Situation = new string('a', SupportRequestValidator.SituationMax + 1) }, SiteStrings.En);
        Assert.Contains("Situation", errors.Keys);
    }

    [Fact]
    public void SupportRequest_AnswersInTheVisitorsLanguage()
    {
        var english = SupportRequestValidator.Validate(ValidRequest() with { FirstName = "" }, SiteStrings.En)["FirstName"];
        var xhosa = SupportRequestValidator.Validate(ValidRequest() with { FirstName = "" }, SiteStrings.Xh)["FirstName"];
        var afrikaans = SupportRequestValidator.Validate(ValidRequest() with { FirstName = "" }, SiteStrings.Af)["FirstName"];

        Assert.Equal(3, new[] { english, xhosa, afrikaans }.Distinct().Count());
    }

    [Fact]
    public void Contact_PassesWithEitherAnEmailOrAPhone()
    {
        Assert.Empty(ContactValidator.Validate(new ContactInput("Nomsa", "nomsa@example.com", "", "Volunteering", "Hello"), SiteStrings.En));
        Assert.Empty(ContactValidator.Validate(new ContactInput("Nomsa", "", "071 123 4567", "Volunteering", "Hello"), SiteStrings.En));
    }

    [Fact]
    public void Contact_NeedsAWayToReply()
    {
        var errors = ContactValidator.Validate(new ContactInput("Nomsa", "", "", "Volunteering", "Hello"), SiteStrings.En);
        Assert.Contains("Email", errors.Keys);
    }

    [Fact]
    public void Contact_RefusesAnInventedSubject() =>
        Assert.Contains("Subject", ContactValidator.Validate(new ContactInput("Nomsa", "nomsa@example.com", "", "Win a prize", "Hello"), SiteStrings.En).Keys);
}

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

    // A name needs a letter, in any alphabet, but nothing more (#27).
    [Theory]
    [InlineData("12345")]
    [InlineData("!!!")]
    [InlineData("-")]
    public void SupportRequest_RefusesANameWithNoLetters(string name)
    {
        var errors = SupportRequestValidator.Validate(ValidRequest() with { FirstName = name, Surname = name }, SiteStrings.En);

        Assert.Equal("Please write your name using letters.", errors["FirstName"]);
        Assert.Contains("Surname", errors.Keys);
    }

    [Theory]
    [InlineData("O'Brien")]
    [InlineData("Mary-Jane")]
    [InlineData("Zoë")]
    [InlineData("Nomsa 2")]
    public void SupportRequest_AcceptsRealNames(string name) =>
        Assert.Empty(SupportRequestValidator.Validate(ValidRequest() with { FirstName = name, Surname = name }, SiteStrings.En));

    [Fact]
    public void Contact_RefusesANameWithNoLetters() =>
        Assert.Contains("Name", ContactValidator.Validate(new ContactInput("12345", "nomsa@example.com", "", "Volunteering", "Hello"), SiteStrings.En).Keys);

    [Fact]
    public void SupportRequest_AcceptsALandline() =>
        Assert.Empty(SupportRequestValidator.Validate(ValidRequest() with { Phone = "021 123 4567" }, SiteStrings.En));
}

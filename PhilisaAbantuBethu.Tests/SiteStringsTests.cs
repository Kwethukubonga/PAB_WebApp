using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class SiteStringsTests
{
    [Theory]
    [InlineData("en", "Home", "Emergency Safehouse")]
    [InlineData("xh", "Ikhaya", "Emergency Safehouse")]
    [InlineData("af", "Tuis", "Veilige Noodhuis")]
    public void EachLanguage_IsFilledFromItsOwnFile(string lang, string home, string firstSupportType)
    {
        var s = SiteStrings.For(lang);

        Assert.Equal(home, s.Nav.Home);
        Assert.Equal(firstSupportType, s.SupportTypes[0]);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("xh")]
    [InlineData("af")]
    public void NoLabel_IsLeftAsItsKey(string lang)
    {
        // A misspelt key comes back as itself (e.g. "Form.Titel"), which would show on the page.
        var s = SiteStrings.For(lang);
        var labels = new object[] { s.Nav, s.Home, s.Form, s.Confirmation }
            .SelectMany(group => group.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => (Name: $"{group.GetType().Name}.{p.Name}", Value: (string)p.GetValue(group)!)))
            .Concat(s.Confirmation.Steps.Select((v, i) => (Name: $"Steps.{i}", Value: v)))
            .Concat(s.SupportTypes.Select((v, i) => (Name: $"SupportTypes.{i}", Value: v)));

        Assert.All(labels, l => Assert.DoesNotMatch(@"^(Nav|Home|Form|Confirmation|SupportTypes)\.", l.Value));
    }

    [Fact]
    public void TheSupportTypes_AreTheOnesPabAskedFor_InThatOrder()
    {
        Assert.Equal(new[]
        {
            "Emergency Safehouse",
            "Youth Support/Youth Empowerment",
            "Youth and GBV support/advice",
            "Report child abuse/ child in need of care and protection",
            "Report older person abuse",
            "Emergency safehouse (LGBTQ+, cis-gender male)",
            "Social work/Internship",
            "General Help / Not Sure"
        }, SiteStrings.En.SupportTypes);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("xh")]
    [InlineData("af")]
    public void EverySupportType_FitsTheDatabaseColumn(string lang)
    {
        // The label is saved as text; a longer one would be refused by PostgreSQL at the last step.
        Assert.All(SiteStrings.For(lang).SupportTypes,
            label => Assert.InRange(label.Length, 1, AppDbContext.ChoiceMax));
    }

    [Fact]
    public void Lists_KeepTheirLength()
    {
        Assert.Equal(8, SiteStrings.En.SupportTypes.Length);
        Assert.Equal(4, SiteStrings.En.Confirmation.Steps.Length);
    }

    [Theory]
    [InlineData("en", true, false, false)]
    [InlineData("xh", false, true, false)]
    [InlineData("af", false, false, true)]
    public void For_ReturnsInstanceMatchingLanguageCode(string lang, bool expectEnglish, bool expectXhosa, bool expectAfrikaans)
    {
        var s = SiteStrings.For(lang);

        Assert.Equal(expectEnglish, s.IsEnglish);
        Assert.Equal(expectXhosa, s.IsXhosa);
        Assert.Equal(expectAfrikaans, s.IsAfrikaans);
    }

    [Fact]
    public void For_DefaultsToEnglish_ForUnknownLanguageCode()
    {
        var s = SiteStrings.For("de");

        Assert.True(s.IsEnglish);
        Assert.Same(SiteStrings.En, s);
    }

    [Fact]
    public void AllThreeLanguages_HaveTheSameNumberOfSupportTypes()
    {
        Assert.Equal(SiteStrings.En.SupportTypes.Length, SiteStrings.Xh.SupportTypes.Length);
        Assert.Equal(SiteStrings.En.SupportTypes.Length, SiteStrings.Af.SupportTypes.Length);
    }

    [Fact]
    public void AllThreeLanguages_HaveTheSameNumberOfConfirmationSteps()
    {
        Assert.Equal(SiteStrings.En.Confirmation.Steps.Length, SiteStrings.Xh.Confirmation.Steps.Length);
        Assert.Equal(SiteStrings.En.Confirmation.Steps.Length, SiteStrings.Af.Confirmation.Steps.Length);
    }
}

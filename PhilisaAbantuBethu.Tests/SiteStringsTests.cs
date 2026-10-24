using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class SiteStringsTests
{
    [Theory]
    [InlineData("en", "Home", "Food Assistance")]
    [InlineData("xh", "Ikhaya", "Inkxaso Yokutya")]
    [InlineData("af", "Tuis", "Voedselbystand")]
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
    public void Lists_KeepTheirLength()
    {
        Assert.Equal(10, SiteStrings.En.SupportTypes.Length);
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

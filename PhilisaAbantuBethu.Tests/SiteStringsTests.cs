using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class SiteStringsTests
{
    [Fact]
    public void Pick_ReturnsEnglish_WhenLangIsEnglish()
    {
        var result = SiteStrings.En.Pick("Home", "Ikhaya", "Tuis");

        Assert.Equal("Home", result);
    }

    [Fact]
    public void Pick_ReturnsXhosa_WhenLangIsXhosa()
    {
        var result = SiteStrings.Xh.Pick("Home", "Ikhaya", "Tuis");

        Assert.Equal("Ikhaya", result);
    }

    [Fact]
    public void Pick_ReturnsAfrikaans_WhenLangIsAfrikaans()
    {
        var result = SiteStrings.Af.Pick("Home", "Ikhaya", "Tuis");

        Assert.Equal("Tuis", result);
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

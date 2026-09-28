using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class ProgrammeTranslationsTests
{
    [Fact]
    public void TitleFor_ReturnsXhosaTranslation_WhenAvailable()
    {
        var prog = SiteData.FindProgramme("youth-programme");

        Assert.Equal("Inkqubo Yolutsha", prog.TitleFor(SiteStrings.Xh));
    }

    [Fact]
    public void TitleFor_ReturnsAfrikaansTranslation_WhenAvailable()
    {
        var prog = SiteData.FindProgramme("youth-programme");

        Assert.Equal("Jeugprogram", prog.TitleFor(SiteStrings.Af));
    }

    [Fact]
    public void TitleFor_ReturnsEnglish_WhenLanguageIsEnglish()
    {
        var prog = SiteData.FindProgramme("youth-programme");

        Assert.Equal(prog.Title, prog.TitleFor(SiteStrings.En));
    }

    [Fact]
    public void TitleFor_FallsBackToEnglish_WhenXhosaTranslationMissing()
    {
        // Men's Café has an Afrikaans translation but no isiXhosa one yet.
        var prog = SiteData.FindProgramme("mens-cafe");

        Assert.Equal(prog.Title, prog.TitleFor(SiteStrings.Xh));
        Assert.NotEqual(prog.Title, prog.TitleFor(SiteStrings.Af));
    }

    [Fact]
    public void ObjectivesFor_ReturnsTranslatedList_WhenAvailable()
    {
        var prog = SiteData.FindProgramme("youth-programme");

        var xhObjectives = prog.ObjectivesFor(SiteStrings.Xh);

        Assert.Equal(prog.Objectives.Count, xhObjectives.Count);
        Assert.NotEqual(prog.Objectives[0], xhObjectives[0]);
    }

    [Fact]
    public void ObjectivesFor_FallsBackToEnglishList_WhenTranslationMissing()
    {
        var prog = SiteData.FindProgramme("mens-cafe");

        var xhObjectives = prog.ObjectivesFor(SiteStrings.Xh);

        Assert.Equal(prog.Objectives, xhObjectives);
    }
}

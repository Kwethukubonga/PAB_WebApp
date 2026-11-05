using PhilisaAbantuBethu.Data;

namespace PhilisaAbantuBethu.Tests;

/// <summary>Content/programmes.json is edited by hand, so a mistake in it is caught here, not on the site (A8).</summary>
public class ProgrammeContentTests
{
    private static readonly string WebRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PhilisaAbantuBethu", "wwwroot"));

    [Fact]
    public void EveryProgramme_HasItsEnglishWords()
    {
        Assert.All(SiteData.Programmes, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Title), $"{p.Id} has no title");
            Assert.False(string.IsNullOrWhiteSpace(p.Overview), $"{p.Id} has no overview");
            Assert.NotEmpty(p.Objectives);
            Assert.NotEmpty(p.Activities);
        });
    }

    [Fact]
    public void Ids_AreUnique() =>
        Assert.Equal(SiteData.Programmes.Count, SiteData.Programmes.Select(p => p.Id).Distinct().Count());

    [Fact]
    public void Translations_AreOnlyForTheSiteLanguages() =>
        Assert.All(SiteData.Programmes, p => Assert.All(p.Translations.Keys, lang => Assert.Contains(lang, new[] { "xh", "af" })));

    [Fact]
    public void Translations_HaveTheSameNumberOfObjectivesAndActivities()
    {
        // A translation with a missing line would quietly drop part of the programme for that language.
        foreach (var p in SiteData.Programmes)
            foreach (var (lang, text) in p.Translations)
            {
                Assert.True(text.Objectives.Length == p.Objectives.Count, $"{p.Id} ({lang}) objectives");
                Assert.True(text.Activities.Length == p.Activities.Count, $"{p.Id} ({lang}) activities");
            }
    }

    [Fact]
    public void EveryPicture_IsOnTheSite()
    {
        foreach (var p in SiteData.Programmes)
            foreach (var path in p.GalleryImages.Prepend(p.ImagePath))
                Assert.True(File.Exists(Path.Combine(WebRoot, path.TrimStart('/'))), $"{p.Id}: {path} is missing from wwwroot");
    }
}

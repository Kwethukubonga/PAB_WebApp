using System.Collections;
using System.Globalization;
using System.Resources;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// Every key in Resources/SharedResource.resx must have an isiXhosa and an Afrikaans value too,
/// or that page would quietly fall back to English for someone who chose another language.
/// </summary>
public class SharedResourceTests
{
    private static Dictionary<string, string> Entries(CultureInfo culture)
    {
        // tryParents: false, so a missing translation isn't hidden by the English fallback.
        var set = Text.Resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.NotNull(set);
        return set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
    }

    [Fact]
    public void EnglishFile_HasText()
    {
        var english = Entries(CultureInfo.InvariantCulture);

        Assert.NotEmpty(english);
        Assert.All(english, e => Assert.False(string.IsNullOrWhiteSpace(e.Value), $"{e.Key} is empty"));
    }

    [Theory]
    [InlineData("xh")]
    [InlineData("af")]
    public void EveryKey_IsTranslated(string lang)
    {
        var english = Entries(CultureInfo.InvariantCulture);
        var translated = Entries(CultureInfo.GetCultureInfo(lang));

        var missing = english.Keys.Where(k => !translated.TryGetValue(k, out var v) || string.IsNullOrWhiteSpace(v)).ToList();
        var extra = translated.Keys.Except(english.Keys).ToList();

        Assert.True(missing.Count == 0, $"Missing in {lang}: {string.Join(", ", missing)}");
        Assert.True(extra.Count == 0, $"In {lang} but not in English: {string.Join(", ", extra)}");
    }

    [Theory]
    [InlineData("en", "Free and confidential.")]
    [InlineData("xh", "Mahhala kwaye yimfihlo.")]
    [InlineData("af", "Gratis en vertroulik.")]
    public void Indexer_ReadsTheChosenLanguage(string lang, string expected) =>
        Assert.Equal(expected, SiteStrings.For(lang)["Home.FreeAndConfidential"]);

    [Fact]
    public void UnknownKey_ComesBackAsItself() =>
        Assert.Equal("No.Such.Key", SiteStrings.Xh["No.Such.Key"]);
}

using System.Globalization;
using System.Resources;

namespace PhilisaAbantuBethu.Localization;

/// <summary>
/// Reads the site's words from Resources/SharedResource.resx (English) and its .xh and .af
/// copies. Keeping every language's text in its own file means a translator can check one file
/// instead of hunting through the pages (A8, W2). The language still comes from
/// <see cref="LangExtensions.CurrentLang"/>, so the switcher and its cookie work as before.
/// </summary>
public static class Text
{
    internal const string BaseName = "PhilisaAbantuBethu.Resources.SharedResource";

    internal static readonly ResourceManager Resources = new(BaseName, typeof(Text).Assembly);

    private static readonly Dictionary<string, CultureInfo> Cultures = new()
    {
        ["en"] = CultureInfo.InvariantCulture,
        ["xh"] = CultureInfo.GetCultureInfo("xh"),
        ["af"] = CultureInfo.GetCultureInfo("af")
    };

    internal static CultureInfo CultureFor(string lang) =>
        Cultures.TryGetValue(lang, out var culture) ? culture : CultureInfo.InvariantCulture;

    /// <summary>
    /// The text for <paramref name="key"/> in <paramref name="lang"/>. A key nobody added comes
    /// back as itself, so it shows up on the page instead of a blank; the tests catch it first.
    /// </summary>
    public static string Get(string key, string lang) =>
        Resources.GetString(key, CultureFor(lang)) ?? key;

    /// <summary>
    /// A numbered list, stored as <c>prefix.0</c>, <c>prefix.1</c> and so on. The English file
    /// decides how many there are, and the test makes sure the other two have the same.
    /// </summary>
    public static string[] List(string prefix, string lang)
    {
        var items = new List<string>();
        for (var i = 0; Resources.GetString($"{prefix}.{i}", CultureInfo.InvariantCulture) is not null; i++)
            items.Add(Get($"{prefix}.{i}", lang));
        return items.ToArray();
    }
}

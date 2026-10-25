using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Localization;

public record ProgrammeText(
    string Title,
    string Tagline,
    string Description,
    string Overview,
    string[] Objectives,
    string[] Activities);

// A programme's words in the visitor's language. The isiXhosa and Afrikaans text comes from
// Content/programmes.json; a programme without a translation falls back to its English text.
public static class ProgrammeTranslations
{
    private static ProgrammeText? Text(Programme p, SiteStrings s) =>
        s.IsEnglish ? null : p.Translations.GetValueOrDefault(s.Lang);

    public static string TitleFor(this Programme p, SiteStrings s) => Text(p, s)?.Title ?? p.Title;
    public static string TaglineFor(this Programme p, SiteStrings s) => Text(p, s)?.Tagline ?? p.Tagline;
    public static string DescriptionFor(this Programme p, SiteStrings s) => Text(p, s)?.Description ?? p.Description;
    public static string OverviewFor(this Programme p, SiteStrings s) => Text(p, s)?.Overview ?? p.Overview;
    public static IReadOnlyList<string> ObjectivesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Objectives ?? p.Objectives;
    public static IReadOnlyList<string> ActivitiesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Activities ?? p.Activities;
}

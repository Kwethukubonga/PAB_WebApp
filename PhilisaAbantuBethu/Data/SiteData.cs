using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data;

public static class SiteData
{
    /// <summary>PAB's programmes, in the order they are shown. The words live in Content/programmes.json.</summary>
    public static readonly List<Programme> Programmes = ProgrammeContent.Load();

    /// <summary>
    /// The programme with this id, or null when there isn't one. There is no fallback: showing the
    /// first programme for a mistyped or old link looked like the wrong page and answered 200 (#25).
    /// </summary>
    public static Programme? FindProgramme(string? id) =>
        Programmes.FirstOrDefault(p => p.Id == id);

}

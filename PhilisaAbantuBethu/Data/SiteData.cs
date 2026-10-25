using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data;

public static class SiteData
{
    /// <summary>PAB's programmes, in the order they are shown. The words live in Content/programmes.json.</summary>
    public static readonly List<Programme> Programmes = ProgrammeContent.Load();

    public static Programme FindProgramme(string? id) =>
        Programmes.FirstOrDefault(p => p.Id == id) ?? Programmes[0];

}

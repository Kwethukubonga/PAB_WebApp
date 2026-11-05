using System.Text.Json;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data;

/// <summary>
/// Reads PAB's programmes from Content/programmes.json (A8). Each programme's words sit in one
/// place in all three languages, so PAB can change a programme, or a translator can check one,
/// without touching any code. The file is built into the app, so it can't go missing on the server.
/// </summary>
public static class ProgrammeContent
{
    private const string ResourceName = "PhilisaAbantuBethu.Content.programmes.json";

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static List<Programme> Load()
    {
        using var stream = typeof(ProgrammeContent).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"{ResourceName} is missing from the build.");
        var entries = JsonSerializer.Deserialize<List<Entry>>(stream, Options)
            ?? throw new InvalidOperationException("Content/programmes.json is empty.");
        return entries.Select(ToProgramme).ToList();
    }

    private static Programme ToProgramme(Entry e)
    {
        // English is required; it is also what any missing translation falls back to.
        var en = e.Text.GetValueOrDefault("en")
            ?? throw new InvalidOperationException($"Programme '{e.Id}' has no English text.");

        return new Programme
        {
            Id = e.Id,
            Title = en.Title,
            Tagline = en.Tagline,
            Description = en.Description,
            Overview = en.Overview,
            Objectives = en.Objectives.ToList(),
            Activities = en.Activities.ToList(),
            ImagePath = e.ImagePath,
            HeroFocalX = e.HeroFocalX,
            HeroFocalY = e.HeroFocalY,
            Accent = e.Accent,
            GalleryImages = e.GalleryImages,
            EmergencyAddress = e.EmergencyAddress,
            EmergencyPhone = e.EmergencyPhone,
            Translations = e.Text.Where(t => t.Key != "en").ToDictionary(t => t.Key, t => t.Value)
        };
    }

    private sealed class Entry
    {
        public string Id { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int HeroFocalX { get; set; } = 50;
        public int HeroFocalY { get; set; } = 50;
        public string Accent { get; set; } = "#6B21A8";
        public List<string> GalleryImages { get; set; } = new();
        public string? EmergencyAddress { get; set; }
        public string? EmergencyPhone { get; set; }
        public Dictionary<string, ProgrammeText> Text { get; set; } = new();
    }
}

using System.Text;

namespace PhilisaAbantuBethu.Forms;

/// <summary>
/// What people type, made safe to store and to show to staff (G7). Text pasted from other apps can
/// carry characters nobody can see: control codes, a null that PostgreSQL refuses outright, or a
/// right-to-left override that makes a name display backwards in the staff list. Those are removed;
/// line breaks, tabs and every alphabet are kept. Emoji are refused by the form checks instead
/// (<see cref="HasEmoji"/>), so the person sees why rather than having them disappear.
/// </summary>
public static class TextInput
{
    public static string? Clean(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        StringBuilder? cleaned = null;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (IsInvisibleTrouble(c))
            {
                cleaned ??= new StringBuilder(value, 0, i, value.Length);
                continue;
            }
            cleaned?.Append(c);
        }
        return cleaned?.ToString() ?? value;
    }

    /// <summary>
    /// How many characters this is as a person counts them: an emoji is one, not the two UTF-16
    /// units .NET stores. It matches how PostgreSQL measures a varchar, so a value that passes here
    /// always fits its column.
    /// </summary>
    public static int Length(string value) => value.EnumerateRunes().Count();

    /// <summary>
    /// Whether the text holds an emoji. No field on the site takes them: staff read every request
    /// and note as plain words, and an emoji in an area or a name helps nobody find anyone. The
    /// browser checks the same ranges (wwwroot/js/form-checks.js).
    /// </summary>
    public static bool HasEmoji(string? value) =>
        !string.IsNullOrEmpty(value) && value.EnumerateRunes().Any(r => IsEmoji(r.Value));

    private static bool IsEmoji(int c) =>
        c is >= 0x1F000 and <= 0x1FAFF   // faces, people, animals, food, flags, symbols
        || c is >= 0x2600 and <= 0x27BF  // sun, hearts, ticks and the other older symbols
        || c is >= 0x2B50 and <= 0x2B55  // star and circles
        || c is 0x231A or 0x231B         // watch, hourglass
        || c is >= 0x23E9 and <= 0x23FA  // media buttons, alarm clock
        || c is 0xFE0F                   // asks for the emoji picture of the character before it
        || c is >= 0xE0020 and <= 0xE007F; // tags inside some flags

    private static bool IsInvisibleTrouble(char c) =>
        (char.IsControl(c) && c is not '\n' and not '\r' and not '\t')
        || c is >= '‪' and <= '‮'   // bidirectional embeddings and overrides
        || c is >= '⁦' and <= '⁩';  // bidirectional isolates
}

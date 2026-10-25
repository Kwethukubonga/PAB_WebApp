using System.Text;

namespace PhilisaAbantuBethu.Forms;

/// <summary>
/// What people type, made safe to store and to show to staff (G7). Text pasted from other apps can
/// carry characters nobody can see: control codes, a null that PostgreSQL refuses outright, or a
/// right-to-left override that makes a name display backwards in the staff list. Those are removed;
/// line breaks, tabs, emoji and every alphabet are kept.
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

    private static bool IsInvisibleTrouble(char c) =>
        (char.IsControl(c) && c is not '\n' and not '\r' and not '\t')
        || c is >= '‪' and <= '‮'   // bidirectional embeddings and overrides
        || c is >= '⁦' and <= '⁩';  // bidirectional isolates
}

using System.Text.RegularExpressions;

namespace PhilisaAbantuBethu.Services;

/// <summary>
/// Checks a phone number against the South African numbering plan set by ICASA. These are the
/// same rules as the Philisa Volunteers Android app (utils/PhoneNumber.kt). The website also
/// allows the dashes, brackets and "+27 (0)" that people type when the keyboard doesn't stop them.
///
/// A South African number is 10 digits: 0, a two digit code, then seven digits, for example
/// 082 123 4567, or +27 82 123 4567. Cellphone codes pass, and so do landlines (021 for Cape Town
/// and the other area codes starting 01 to 05): someone with only a landline must still be able to
/// ask for help, and PAB can phone them back (#27). Toll-free and share-call numbers (0800, 0861)
/// don't, since nobody answers them. People from abroad can use an international number starting
/// with + (or 00) and their country code.
/// </summary>
public static partial class PhoneNumber
{
    // Network codes after the leading 0. A number can move to another network, so the code
    // only shows the network it started on.
    private static readonly HashSet<string> MobileCodes = new()
    {
        "72", "76", "79", "82",                                // Vodacom
        "73", "78", "83",                                      // MTN
        "74", "84",                                            // Cell C
        "81",                                                  // mainly Telkom
        "60", "61", "62", "63", "64", "65", "66", "67", "68", "71" // shared newer blocks
    };

    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex LocalShape();

    // Geographic (landline) numbers: area codes 01x to 05x, such as 021 Cape Town or 011 Johannesburg.
    [GeneratedRegex(@"^0[1-5]")]
    private static partial Regex Landline();

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex International();

    public static bool IsValid(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var compact = Compact(input);

        var local = SouthAfricanLocal(compact);
        if (local is null) return International().IsMatch(compact);

        if (!LocalShape().IsMatch(local)) return false;
        if (!MobileCodes.Contains(local.Substring(1, 2)) && !Landline().IsMatch(local)) return false;

        // The right shape, but plainly made up, like 082 222 2222.
        return local[3..].Distinct().Count() > 1;
    }

    /// <summary>
    /// How a number is stored. A South African cellphone number always reads 082 123 4567, however
    /// it was typed (+27 82..., 082-123-4567, (082) 123 4567), so staff see one format (G7). Any
    /// other number is trimmed and keeps single spaces, the way it was written.
    /// </summary>
    public static string Tidy(string? input)
    {
        var spaced = string.Join(' ', (input ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (!IsValid(spaced)) return spaced;

        var local = SouthAfricanLocal(Compact(spaced));
        return local is null ? spaced : $"{local[..3]} {local[3..6]} {local[6..]}";
    }

    // Spaces, dashes, brackets and dots (082.123.4567) are how people group the digits.
    private static string Compact(string input)
    {
        var compact = string.Concat(input.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '(' && c != ')' && c != '.'));

        // 00 is how an international number is dialled from a landline: 0027 82... is +27 82...
        if (compact.StartsWith("00")) return "+" + compact[2..];
        // 27 82 123 4567 without the plus, the way people copy it from WhatsApp.
        if (compact.Length == 11 && compact.StartsWith("27") && compact.All(char.IsDigit)) return "+" + compact;
        return compact;
    }

    /// <summary>The number in its 0XX form when it is South African, otherwise null.</summary>
    private static string? SouthAfricanLocal(string compact)
    {
        if (compact.StartsWith("+27"))
        {
            // Also accepts the written form +27 (0)82 123 4567, which the site itself uses.
            var rest = compact[3..];
            return rest.StartsWith('0') ? rest : "0" + rest;
        }
        return compact.StartsWith('0') ? compact : null;
    }
}

using System.Text.RegularExpressions;

namespace PhilisaAbantuBethu.Services;

/// <summary>
/// Checks a phone number against the South African numbering plan set by ICASA.
///
/// Only South African numbers are taken, since PAB works in the Western Cape and phones people
/// back from here. A number is 10 digits, 0 then nine more (0821234567), or 12 characters, +27
/// then the nine digits without the 0 (+27821234567). The box in the browser takes nothing else;
/// spaces, dashes and brackets sent without the script (082 123 4567) are ignored here.
/// Cellphone codes pass, and so do landlines (021 for Cape Town and the other area codes starting
/// 01 to 05): someone with only a landline must still be able to ask for help, and PAB can phone
/// them back (#27). Toll-free and share-call numbers (0800, 0861) don't, since nobody answers them.
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

    // 0821234567 or +27821234567.
    [GeneratedRegex(@"^(0|\+27)[1-9]\d{8}$")]
    private static partial Regex Shape();

    // Geographic (landline) numbers: area codes 01x to 05x, such as 021 Cape Town or 011 Johannesburg.
    [GeneratedRegex(@"^0[1-5]")]
    private static partial Regex Landline();

    public static bool IsValid(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var compact = Compact(input);
        if (!Shape().IsMatch(compact)) return false;

        var local = SouthAfricanLocal(compact);
        if (!MobileCodes.Contains(local.Substring(1, 2)) && !Landline().IsMatch(local)) return false;

        // The right shape, but plainly made up, like 082 222 2222.
        return local[3..].Distinct().Count() > 1;
    }

    /// <summary>
    /// How a number is stored. It always reads 082 123 4567, however it was typed (+27821234567,
    /// 082-123-4567), so staff see one format (G7). A number that isn't valid is only trimmed.
    /// </summary>
    public static string Tidy(string? input)
    {
        var spaced = string.Join(' ', (input ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (!IsValid(spaced)) return spaced;

        var local = SouthAfricanLocal(Compact(spaced));
        return $"{local[..3]} {local[3..6]} {local[6..]}";
    }

    // Spaces, dashes and brackets are how people group the digits.
    private static string Compact(string input) =>
        string.Concat(input.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '(' && c != ')'));

    /// <summary>The number in its 0XX form.</summary>
    private static string SouthAfricanLocal(string compact) =>
        compact.StartsWith("+27") ? "0" + compact[3..] : compact;
}

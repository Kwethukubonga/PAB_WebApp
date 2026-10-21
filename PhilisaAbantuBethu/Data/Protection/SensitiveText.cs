using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PhilisaAbantuBethu.Data.Protection;

public interface ISensitiveText
{
    /// <summary>Encrypts on the way into the database and decrypts on the way out.</summary>
    ValueConverter<string, string> Converter { get; }

    string Protect(string plain);
    string Unprotect(string stored);
}

/// <summary>
/// Encrypts the columns that hold what someone wrote about their situation, so a copy of the
/// database file or a backup can't be read (A5). The key comes from ASP.NET Data Protection,
/// which on Railway is kept on the volume at DataProtection:KeysPath, so a redeploy can still
/// read what was saved before it.
///
/// Text saved before this was added, and text written by the EF design-time tools, is stored in
/// the clear. Anything that doesn't carry the marker below is handed back unchanged rather than
/// failing, so old rows still open.
/// </summary>
public class SensitiveText : ISensitiveText
{
    // Marks a value this class encrypted. Without it the value is older plain text.
    internal const string Marker = "enc:v1:";

    private readonly IDataProtector _protector;
    private readonly ILogger<SensitiveText> _log;

    public SensitiveText(IDataProtectionProvider provider, ILogger<SensitiveText> log)
    {
        _protector = provider.CreateProtector("PhilisaAbantuBethu.SupportRequest.v1");
        _log = log;
        Converter = new ValueConverter<string, string>(plain => Protect(plain), stored => Unprotect(stored));
    }

    public ValueConverter<string, string> Converter { get; }

    public string Protect(string plain) =>
        string.IsNullOrEmpty(plain) ? "" : Marker + _protector.Protect(plain);

    public string Unprotect(string stored)
    {
        if (string.IsNullOrEmpty(stored)) return "";
        if (!stored.StartsWith(Marker, StringComparison.Ordinal)) return stored;

        try
        {
            return _protector.Unprotect(stored[Marker.Length..]);
        }
        catch (Exception ex)
        {
            // A lost or rotated-away key. Say so without putting the row's contents in the log.
            _log.LogError(ex, "A support request's text could not be decrypted with the current keys");
            return "";
        }
    }
}

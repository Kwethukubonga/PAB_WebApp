using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;

namespace PhilisaAbantuBethu.Data.Protection;

/// <summary>
/// Where the data protection keys are kept. These keys encrypt the situation text in the database
/// and sign the staff sign-in cookie and the form tokens, so losing them makes saved support
/// requests unreadable (A5).
///
/// With a Postgres database the keys go in the database, because the hosting disk is wiped on a
/// restart. Keys stored next to the data they protect would protect nothing, so they are
/// encrypted with a certificate whose private half lives only in the host's secret settings:
/// <c>DataProtection:Certificate</c> (the .pfx file as base64, or <c>DataProtection:CertificatePath</c>
/// for a file) and <c>DataProtection:CertificatePassword</c>. A copy of the database alone can't
/// then be read. Without Postgres the keys stay in files at <c>DataProtection:KeysPath</c>.
/// </summary>
public static class DataProtectionSetup
{
    public const string Section = "DataProtection";

    public static IDataProtectionBuilder AddSiteDataProtection(
        this IServiceCollection services, IConfiguration config, IHostEnvironment environment, bool keysInDatabase)
    {
        var builder = services.AddDataProtection().SetApplicationName("PhilisaAbantuBethu");

        if (!keysInDatabase)
        {
            // Keys on a volume, as before. Without a path they live in the app's default location.
            var keysPath = config[$"{Section}:KeysPath"];
            if (!string.IsNullOrWhiteSpace(keysPath))
            {
                builder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
            }
            return builder;
        }

        builder.PersistKeysToDbContext<PostgresAppDbContext>();

        var certificate = LoadCertificate(config);
        if (certificate is not null)
        {
            builder.ProtectKeysWithCertificate(certificate);
        }
        else if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            // Better that a new version refuses to start (the host then keeps serving the old one)
            // than that it quietly stores the keys unprotected next to the text they decrypt.
            throw new InvalidOperationException(
                $"The keys are stored in the database, so {Section}:Certificate and {Section}:CertificatePassword " +
                "must be set. Generate them with tools/data-protection/New-KeyCertificate.ps1.");
        }

        return builder;
    }

    /// <summary>The certificate from the settings, or null when none is configured.</summary>
    internal static X509Certificate2? LoadCertificate(IConfiguration config)
    {
        var path = config[$"{Section}:CertificatePath"];
        var base64 = config[$"{Section}:Certificate"];
        var password = config[$"{Section}:CertificatePassword"];

        byte[] bytes;
        if (!string.IsNullOrWhiteSpace(path))
        {
            bytes = File.ReadAllBytes(path);
        }
        else if (!string.IsNullOrWhiteSpace(base64))
        {
            try
            {
                bytes = Convert.FromBase64String(base64.Trim());
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"{Section}:Certificate is not valid base64.", ex);
            }
        }
        else
        {
            return null;
        }

        try
        {
            // Ephemeral: the private key stays in memory instead of being written to disk.
            return X509CertificateLoader.LoadPkcs12(bytes, password, X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            throw new InvalidOperationException(
                $"The data protection certificate could not be opened. Check {Section}:CertificatePassword.", ex);
        }
    }
}

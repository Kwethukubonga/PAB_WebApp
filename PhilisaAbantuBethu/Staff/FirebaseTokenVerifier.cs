using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace PhilisaAbantuBethu.Staff;

/// <summary>Who a valid Firebase ID token belongs to.</summary>
public record FirebaseUser(string Uid, string? Email, string? Name);

public interface IFirebaseTokenVerifier
{
    /// <summary>The token's owner, or null when the token is fake, expired or for another project.</summary>
    Task<FirebaseUser?> VerifyAsync(string idToken, CancellationToken ct = default);
}

/// <summary>
/// Checks a Firebase ID token the way Firebase documents it: signed by Google's securetoken
/// keys, issued for this project, and not expired. The keys are fetched from Google and cached.
/// </summary>
public class FirebaseTokenVerifier : IFirebaseTokenVerifier
{
    private readonly FirebaseOptions _options;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _keys;
    private readonly JsonWebTokenHandler _handler = new();
    private readonly ILogger<FirebaseTokenVerifier> _log;

    public FirebaseTokenVerifier(IOptions<FirebaseOptions> options, ILogger<FirebaseTokenVerifier> log)
    {
        _options = options.Value;
        _log = log;
        _keys = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://securetoken.google.com/{_options.ProjectId}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
    }

    public async Task<FirebaseUser?> VerifyAsync(string idToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idToken)) return null;

        var config = await _keys.GetConfigurationAsync(ct);
        var result = await _handler.ValidateTokenAsync(idToken, new TokenValidationParameters
        {
            ValidIssuer = $"https://securetoken.google.com/{_options.ProjectId}",
            ValidAudience = _options.ProjectId,
            IssuerSigningKeys = config.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        });

        if (!result.IsValid)
        {
            _log.LogWarning("Staff sign-in refused an invalid Firebase token: {Reason}", result.Exception?.GetType().Name);
            return null;
        }

        var token = (JsonWebToken)result.SecurityToken;
        token.TryGetPayloadValue<string>("email", out var email);
        token.TryGetPayloadValue<string>("name", out var name);
        return new FirebaseUser(token.Subject, email, name);
    }
}

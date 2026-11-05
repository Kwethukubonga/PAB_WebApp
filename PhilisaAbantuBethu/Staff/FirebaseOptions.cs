namespace PhilisaAbantuBethu.Staff;

/// <summary>
/// The Firebase project PAB staff sign in with: the same one as the Philisa Volunteers app, so
/// staff use the admin accounts they already have. None of these values are secret. The API key
/// comes from a Web app registered under Firebase console > Project settings > Your apps.
/// </summary>
public class FirebaseOptions
{
    public const string Section = "Firebase";

    public string ProjectId { get; set; } = "pab-volunteers";
    public string ApiKey { get; set; } = "";
    public string? AuthDomain { get; set; }

    public string ResolvedAuthDomain => string.IsNullOrWhiteSpace(AuthDomain) ? $"{ProjectId}.firebaseapp.com" : AuthDomain!;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ProjectId);
}

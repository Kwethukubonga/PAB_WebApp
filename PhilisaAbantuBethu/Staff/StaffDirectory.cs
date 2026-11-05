using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PhilisaAbantuBethu.Staff;

public interface IStaffDirectory
{
    /// <summary>True when this person is a PAB admin.</summary>
    Task<bool> IsStaffAsync(FirebaseUser user, string idToken, CancellationToken ct = default);
}

/// <summary>
/// PAB staff are the people with role "admin" on their users record in Firestore, the same rule
/// the Philisa Volunteers app uses. The record is read through Firestore's REST API with the
/// person's own ID token, which the security rules already allow, so this site needs no server
/// key and nobody has to keep a second list of staff up to date.
/// </summary>
public class FirestoreStaffDirectory : IStaffDirectory
{
    private readonly HttpClient _http;
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirestoreStaffDirectory> _log;

    public FirestoreStaffDirectory(HttpClient http, IOptions<FirebaseOptions> options, ILogger<FirestoreStaffDirectory> log)
    {
        _http = http;
        _options = options.Value;
        _log = log;
    }

    public async Task<bool> IsStaffAsync(FirebaseUser user, string idToken, CancellationToken ct = default)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{_options.ProjectId}/databases/(default)/documents/users/{Uri.EscapeDataString(user.Uid)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        using var response = await _http.SendAsync(request, ct);
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden) return false;
        if (!response.IsSuccessStatusCode)
        {
            _log.LogWarning("Could not read the staff record for {Uid}: {Status}", user.Uid, (int)response.StatusCode);
            return false;
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.TryGetProperty("fields", out var fields)
            && fields.TryGetProperty("role", out var role)
            && role.TryGetProperty("stringValue", out var value)
            && value.GetString() == "admin";
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace PhilisaAbantuBethu.Staff;

/// <summary>
/// How the staff API endpoints decide who is asking. The Razor staff pages use a sign-in cookie
/// (<see cref="StaffAuth"/>); the API instead takes the Firebase ID token the Android app already
/// holds, in an <c>Authorization: Bearer</c> header, so an admin's phone can call it without a
/// second sign-in. The token is checked the same way for both (<see cref="IFirebaseTokenVerifier"/>).
/// </summary>
public static class StaffApiAuth
{
    public const string Scheme = "StaffBearer";
    public const string Policy = "StaffApi";

    /// <summary>The raw bearer token on this request, or null when there isn't one.</summary>
    public static string? BearerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : null;
    }
}

/// <summary>
/// Reads the bearer token, checks Google signed it for this Firebase project, and signs the
/// caller in for this one request. Whether they are a PAB admin is a separate question, answered
/// by <see cref="StaffRoleHandler"/>, so a volunteer's valid token gets 403 rather than 401.
/// </summary>
public class StaffBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TokenItemKey = "pab_staff_token";

    private readonly IFirebaseTokenVerifier _verifier;

    public StaffBearerHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        IFirebaseTokenVerifier verifier) : base(options, logger, encoder) => _verifier = verifier;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = StaffApiAuth.BearerToken(Request);
        if (token is null) return AuthenticateResult.NoResult();

        var user = await _verifier.VerifyAsync(token, Context.RequestAborted);
        if (user is null) return AuthenticateResult.Fail("The sign-in token is not valid.");

        // The role check needs the token again to read Firestore as this person.
        Context.Items[TokenItemKey] = token;

        // Deliberately NOT StaffAuth.PrincipalFor: that carries the staff claim, which the pages
        // only add once the role has been checked. A valid token proves who someone is, not that
        // they work for PAB, so a volunteer's token must not look like a staff sign-in here.
        return AuthenticateResult.Success(new AuthenticationTicket(SignedInAs(user), StaffApiAuth.Scheme));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers[HeaderNames.WWWAuthenticate] = "Bearer";
        return base.HandleChallengeAsync(properties);
    }

    /// <summary>Who the token says they are, and nothing about what they may do.</summary>
    private static ClaimsPrincipal SignedInAs(FirebaseUser user)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Uid) };
        if (!string.IsNullOrWhiteSpace(user.Email)) claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrWhiteSpace(user.Name)) claims.Add(new Claim(ClaimTypes.Name, user.Name));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, StaffApiAuth.Scheme));
    }
}

/// <summary>Being signed in is not enough: the caller must be a PAB admin.</summary>
public class StaffRoleRequirement : IAuthorizationRequirement;

public class StaffRoleHandler : AuthorizationHandler<StaffRoleRequirement>
{
    private readonly IStaffDirectory _directory;
    private readonly IHttpContextAccessor _http;

    public StaffRoleHandler(IStaffDirectory directory, IHttpContextAccessor http)
    {
        _directory = directory;
        _http = http;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, StaffRoleRequirement requirement)
    {
        // The staff pages already checked the role at sign-in and put it in the cookie. Both
        // schemes may have run, so every identity is looked at, not just the first.
        if (context.User.Identities.Any(id =>
                id.AuthenticationType == StaffAuth.Scheme
                && id.HasClaim(StaffAuth.StaffClaim, "true")))
        {
            context.Succeed(requirement);
            return;
        }

        var http = _http.HttpContext;
        var uid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (http is null || string.IsNullOrEmpty(uid)) return;
        if (http.Items[StaffBearerHandler.TokenItemKey] is not string token) return;

        var user = new FirebaseUser(uid, context.User.FindFirstValue(ClaimTypes.Email), context.User.FindFirstValue(ClaimTypes.Name));
        if (await _directory.IsStaffAsync(user, token, http.RequestAborted)) context.Succeed(requirement);
    }
}

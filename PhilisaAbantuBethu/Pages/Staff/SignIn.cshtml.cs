using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Pages.Staff;

/// <summary>
/// Staff sign in with their Philisa Volunteers admin account. The browser signs in to Firebase and
/// sends the ID token here; the server checks it, checks the admin role, and starts its own
/// session. Nothing about the Firebase sign-in is kept in the browser afterwards.
/// </summary>
[AllowAnonymous]
public class SignInModel : StaffPageModel
{
    private const string DefaultReturnUrl = "/Staff/Requests";
    private static readonly TimeSpan SessionLength = TimeSpan.FromHours(8);

    private readonly IFirebaseTokenVerifier _verifier;
    private readonly IStaffDirectory _directory;
    private readonly ILogger<SignInModel> _log;

    public SignInModel(IFirebaseTokenVerifier verifier, IStaffDirectory directory, IOptions<FirebaseOptions> firebase, ILogger<SignInModel> log)
    {
        _verifier = verifier;
        _directory = directory;
        _log = log;
        Firebase = firebase.Value;
    }

    public FirebaseOptions Firebase { get; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool IsSignedIn => User.HasClaim(StaffAuth.StaffClaim, "true");

    private string SafeReturnUrl => !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : DefaultReturnUrl;

    public IActionResult OnGet() => IsSignedIn ? LocalRedirect(SafeReturnUrl) : Page();

    public async Task<IActionResult> OnPostSessionAsync([FromForm] string? idToken)
    {
        var s = HttpContext.Strings();
        var ct = HttpContext.RequestAborted;

        FirebaseUser? user;
        bool isStaff;
        try
        {
            user = await _verifier.VerifyAsync(idToken ?? "", ct);
            isStaff = user is not null && await _directory.IsStaffAsync(user, idToken!, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            _log.LogError(ex, "Staff sign-in could not reach Google");
            return Error(StatusCodes.Status503ServiceUnavailable, s["Staff.SignIn.SignIsntAvailableRight"]);
        }

        if (user is null)
        {
            return Error(StatusCodes.Status401Unauthorized, s["Staff.SignIn.SignDidntWorkPlease"]);
        }

        if (!isStaff)
        {
            _log.LogWarning("Staff sign-in refused for {Uid}: not an admin", user.Uid);
            return Error(StatusCodes.Status403Forbidden, s["Staff.SignIn.AccountIsntPABStaff"]);
        }

        await HttpContext.SignInAsync(StaffAuth.Scheme, StaffAuth.PrincipalFor(user), new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(SessionLength)
        });
        _log.LogInformation("Staff member {Uid} signed in", user.Uid);

        return new JsonResult(new { redirect = SafeReturnUrl });
    }

    // The sign-in script reads the message and shows it above the form.
    private static JsonResult Error(int status, string message) =>
        new(new { error = message }) { StatusCode = status };

    public async Task<IActionResult> OnPostSignOutAsync()
    {
        await HttpContext.SignOutAsync(StaffAuth.Scheme);
        return RedirectToPage();
    }
}

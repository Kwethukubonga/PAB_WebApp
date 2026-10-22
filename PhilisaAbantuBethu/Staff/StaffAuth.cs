using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhilisaAbantuBethu.Staff;

public static class StaffAuth
{
    public const string Scheme = "StaffCookie";
    public const string Policy = "Staff";
    public const string StaffClaim = "pab_staff";

    public static ClaimsPrincipal PrincipalFor(FirebaseUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Uid),
            new(StaffClaim, "true")
        };
        if (!string.IsNullOrWhiteSpace(user.Email)) claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrWhiteSpace(user.Name)) claims.Add(new Claim(ClaimTypes.Name, user.Name));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }

    /// <summary>A name to show and to record against status changes: the email, or the uid if there is none.</summary>
    public static string DisplayName(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "staff";
}

/// <summary>
/// Base for the staff pages. Requests can describe abuse, so the pages are never stored in a
/// browser or proxy cache, never indexed, and leave out the public quick exit button.
/// </summary>
public abstract class StaffPageModel : PageModel
{
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        ViewData["HideQuickExit"] = true;
        ViewData["NoIndex"] = true;
        base.OnPageHandlerExecuting(context);
    }
}

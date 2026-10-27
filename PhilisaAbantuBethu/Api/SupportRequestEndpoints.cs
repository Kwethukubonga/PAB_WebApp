using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

namespace PhilisaAbantuBethu.Api;

/// <summary>What a client sends to ask PAB for support. The same fields as the web form.</summary>
public record SupportRequestBody(
    string? FirstName,
    string? Surname,
    string? Phone,
    string? Email,
    string? Area,
    string? ContactMethod,
    string? SupportType,
    string? Situation,
    string? Urgent,
    string? Extra,
    bool Declaration)
{
    public SupportRequestInput ToInput() => new(
        FirstName, Surname, Phone, Email, Area, ContactMethod, SupportType, Situation, Urgent, Extra, Declaration);
}

/// <summary>What the client gets back once the request is saved.</summary>
public record SupportRequestAccepted(string Reference);

/// <summary>A request as staff see it in a list: no situation text, so a list can't leak it.</summary>
public record SupportRequestSummary(
    int Id,
    string Reference,
    string FirstName,
    string Surname,
    string Area,
    string SupportType,
    string ContactMethod,
    bool Urgent,
    string Status,
    DateTime SubmittedAt,
    DateTime? StatusUpdatedAt,
    string? HandledBy)
{
    public static SupportRequestSummary From(SupportRequest r) => new(
        r.Id, r.Reference, r.FirstName, r.Surname, r.Area, r.SupportType, r.ContactMethod,
        r.IsUrgent, r.Status, r.SubmittedAt, r.StatusUpdatedAt, r.HandledBy);
}

/// <summary>The whole request, including what the person wrote. Staff only, one at a time.</summary>
public record SupportRequestDetail(
    int Id,
    string Reference,
    string FirstName,
    string Surname,
    string Phone,
    string Email,
    string Area,
    string SupportType,
    string ContactMethod,
    string Situation,
    string Extra,
    bool Urgent,
    string Status,
    DateTime SubmittedAt,
    DateTime? StatusUpdatedAt,
    string? HandledBy,
    string? StaffNotes)
{
    public static SupportRequestDetail From(SupportRequest r) => new(
        r.Id, r.Reference, r.FirstName, r.Surname, r.Phone, r.Email, r.Area, r.SupportType,
        r.ContactMethod, r.Situation, r.Extra, r.IsUrgent, r.Status, r.SubmittedAt,
        r.StatusUpdatedAt, r.HandledBy, r.StaffNotes);
}

public record SupportRequestPage(IReadOnlyList<SupportRequestSummary> Items, int Page, int PageSize, int Total)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}

public record StatusChange(string? Status, string? StaffNotes);

/// <summary>
/// The support request endpoints. Sending one is public, because somebody in danger must not have
/// to make an account first; reading and changing them is for PAB admins only (B1).
/// </summary>
public static class SupportRequestEndpoints
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static RouteGroupBuilder MapSupportRequests(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/support-requests").WithTags("Support requests");

        group.MapPost("/", Submit)
            .AllowAnonymous()
            .WithMetadata(new Security.LimitFormSendsAttribute())
            .WithSummary("Ask PAB for support")
            .WithDescription("Open to anyone. Rate-limited per address, the same as the web form.");

        group.MapGet("/", List)
            .RequireAuthorization(StaffApiAuth.Policy)
            .WithSummary("List requests (PAB admins)");

        group.MapGet("/{id:int}", Detail)
            .RequireAuthorization(StaffApiAuth.Policy)
            .WithSummary("One request in full (PAB admins)");

        group.MapPatch("/{id:int}", ChangeStatus)
            .RequireAuthorization(StaffApiAuth.Policy)
            .WithSummary("Move a request to another status (PAB admins)");

        return group;
    }

    private static async Task<Results<Created<SupportRequestAccepted>, ValidationProblem>> Submit(
        SupportRequestBody body,
        ISupportRequestService requests,
        HttpContext http,
        CancellationToken ct)
    {
        // Error messages come back in the language the caller asked for, as on the web form.
        var errors = SupportRequestValidator.Validate(body.ToInput().Cleaned(), http.Strings());
        if (errors.Count > 0) return ApiProblem.Validation(errors);

        var reference = await requests.SubmitAsync(body.ToInput().Cleaned(), ct);
        return TypedResults.Created($"/api/support-requests/by-reference/{reference}", new SupportRequestAccepted(reference));
    }

    private static async Task<Results<Ok<SupportRequestPage>, ValidationProblem>> List(
        ISupportRequestService requests,
        string? status,
        bool? urgent,
        int? page,
        int? pageSize,
        CancellationToken ct)
    {
        if (status is not null && !SupportRequestStatus.All.Contains(status))
        {
            return ApiProblem.Validation(
                new Dictionary<string, string> { ["status"] = $"Use one of: {string.Join(", ", SupportRequestStatus.All)}." },
                "The status filter is not one we use.");
        }

        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var number = Math.Max(page ?? 1, 1);

        var (items, total) = await requests.PageAsync(status, urgent, number, size, ct);
        return TypedResults.Ok(new SupportRequestPage(
            items.Select(SupportRequestSummary.From).ToList(), number, size, total));
    }

    private static async Task<Ok<SupportRequestDetail>> Detail(int id, ISupportRequestService requests, CancellationToken ct)
    {
        var request = await requests.FindAsync(id, ct)
            ?? throw ApiException.NotFound($"There is no support request {id}.");
        return TypedResults.Ok(SupportRequestDetail.From(request));
    }

    private static async Task<Results<Ok<SupportRequestSummary>, ValidationProblem>> ChangeStatus(
        int id,
        StatusChange change,
        ISupportRequestService requests,
        HttpContext http,
        CancellationToken ct)
    {
        if (change.Status is null || !SupportRequestStatus.All.Contains(change.Status))
        {
            return ApiProblem.Validation(
                new Dictionary<string, string> { ["status"] = $"Use one of: {string.Join(", ", SupportRequestStatus.All)}." });
        }
        if (change.StaffNotes?.Length > SupportRequestValidator.StaffNotesMax)
        {
            return ApiProblem.Validation(new Dictionary<string, string>
            {
                ["staffNotes"] = $"Please keep notes under {SupportRequestValidator.StaffNotesMax} characters."
            });
        }

        var staff = StaffAuth.DisplayName(http.User);
        var updated = await requests.SetStatusAsync(id, change.Status, staff, change.StaffNotes, ct)
            ?? throw ApiException.NotFound($"There is no support request {id}.");
        return TypedResults.Ok(SupportRequestSummary.From(updated));
    }
}

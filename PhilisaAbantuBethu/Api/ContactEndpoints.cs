using Microsoft.AspNetCore.Http.HttpResults;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Api;

/// <summary>A general message to the PAB office. Not the way to ask for support.</summary>
public record ContactBody(string? Name, string? Email, string? Phone, string? Subject, string? Message)
{
    public ContactInput ToInput() => new(Name, Email, Phone, Subject, Message);
}

public static class ContactEndpoints
{
    public static RouteGroupBuilder MapContact(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/contact").WithTags("Contact");

        group.MapPost("/", Send)
            .AllowAnonymous()
            .WithMetadata(new Security.LimitFormSendsAttribute())
            .WithSummary("Send the PAB office a message")
            .WithDescription("Rate-limited per address, the same as the web form.");

        return group;
    }

    private static async Task<Results<NoContent, ValidationProblem>> Send(
        ContactBody body,
        IContactService contact,
        HttpContext http,
        CancellationToken ct)
    {
        var errors = ContactValidator.Validate(body.ToInput().Cleaned(), http.Strings());
        if (errors.Count > 0) return ApiProblem.Validation(errors);

        await contact.SubmitAsync(body.ToInput().Cleaned(), ct);
        return TypedResults.NoContent();
    }
}

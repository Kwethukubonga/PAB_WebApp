using Microsoft.AspNetCore.Http.HttpResults;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Api;

public record ProgrammeSummary(string Id, string Title, string Tagline, string Description, string ImagePath, string Accent);

public record ProgrammeDetailBody(
    string Id,
    string Title,
    string Tagline,
    string Description,
    string Overview,
    string ImagePath,
    string Accent,
    IReadOnlyList<string> Objectives,
    IReadOnlyList<string> Activities,
    IReadOnlyList<string> GalleryImages,
    string? EmergencyAddress,
    string? EmergencyPhone);

public record ResourceCategoryBody(string Id, string Name, string Icon, string Colour, List<string> Items);

public record ResourceQuestionBody(string Question, string Answer);

public record ResourcesBody(List<ResourceCategoryBody> Categories, List<ResourceQuestionBody> Questions);

/// <summary>
/// The parts of the site anyone may read: the programmes and the resource lists. Both return the
/// same content the pages show, in the language asked for with <c>?lang=en|xh|af</c>, and both
/// may be cached, so the Android app or an offline page can hold a copy (B1, W11).
/// </summary>
public static class ContentEndpoints
{
    // The content changes rarely and is not personal, so a shared cache may hold it.
    private const string CacheFor = "public, max-age=3600";

    public static void MapContent(this IEndpointRouteBuilder routes)
    {
        var programmes = routes.MapGroup("/api/programmes").WithTags("Programmes");

        programmes.MapGet("/", (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = CacheFor;
            var s = http.Strings();
            return TypedResults.Ok(SiteData.Programmes.Select(p => new ProgrammeSummary(
                p.Id, p.TitleFor(s), p.TaglineFor(s), p.DescriptionFor(s), p.ImagePath, p.Accent)).ToList());
        })
        .AllowAnonymous()
        .WithSummary("Every PAB programme");

        programmes.MapGet("/{id}", Results<Ok<ProgrammeDetailBody>, ProblemHttpResult> (string id, HttpContext http) =>
        {
            // FindProgramme falls back to the first programme, so the id is checked here.
            var programme = SiteData.FindProgramme(id);
            if (programme.Id != id)
            {
                return ApiProblem.Problem(StatusCodes.Status404NotFound, $"There is no programme called \"{id}\".");
            }

            http.Response.Headers.CacheControl = CacheFor;
            var s = http.Strings();
            return TypedResults.Ok(new ProgrammeDetailBody(
                programme.Id,
                programme.TitleFor(s),
                programme.TaglineFor(s),
                programme.DescriptionFor(s),
                programme.OverviewFor(s),
                programme.ImagePath,
                programme.Accent,
                programme.ObjectivesFor(s),
                programme.ActivitiesFor(s),
                programme.GalleryImages,
                programme.EmergencyAddress,
                programme.EmergencyPhone));
        })
        .AllowAnonymous()
        .WithSummary("One programme in full");

        var resources = routes.MapGroup("/api/resources").WithTags("Resources");

        resources.MapGet("/", (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = CacheFor;
            var lang = http.CurrentLang();
            return TypedResults.Ok(new ResourcesBody(
                ResourceData.Categories.Select(c => new ResourceCategoryBody(
                    c.Id, c.Name.For(lang), c.Icon, c.Colour,
                    c.Items.Select(i => i.For(lang)).ToList())).ToList(),
                ResourceData.Questions.Select(q => new ResourceQuestionBody(
                    q.Question.For(lang), q.Answer.For(lang))).ToList()));
        })
        .AllowAnonymous()
        .WithSummary("Emergency numbers, services and questions");
    }
}

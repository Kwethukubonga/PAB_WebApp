using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace PhilisaAbantuBethu.Tests;

/// <summary>Minimal PageContext setup so PageModel.ModelState, Page() and RedirectToPage() work outside the real request pipeline.</summary>
internal static class PageModelTestHelpers
{
    public static PageContext CreatePageContext()
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);

        return new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
        };
    }

    /// <summary>TempData kept in memory, for handlers that pass something to the next page.</summary>
    public static ITempDataDictionary CreateTempData() =>
        new TempDataDictionary(new DefaultHttpContext(), new InMemoryTempDataProvider());

    private sealed class InMemoryTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context) => _values;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => _values = values;
    }
}

/// <summary>
/// Stands in for the form-token check in tests that call a page handler directly, where there is
/// no real request to carry a token. The real check is covered through the whole site in
/// EdgeCaseTests.
/// </summary>
internal class AcceptingAntiforgery : Microsoft.AspNetCore.Antiforgery.IAntiforgery
{
    public Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => new("request", "cookie", "form", "header");
    public Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet GetTokens(HttpContext httpContext) => new("request", "cookie", "form", "header");
    public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);
    public Task ValidateRequestAsync(HttpContext httpContext) => Task.CompletedTask;
    public void SetCookieTokenAndHeader(HttpContext httpContext) { }
}

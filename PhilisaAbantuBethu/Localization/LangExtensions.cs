namespace PhilisaAbantuBethu.Localization;

public static class LangExtensions
{
    public const string CookieName = "pab_lang";
    private const string ItemKey = "pab_lang";
    private static readonly string[] SupportedLangs = { "en", "xh", "af" };

    // Gets the current language for the request, based on query string, cookie, or defaulting to "en".

    public static string CurrentLang(this HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(ItemKey, out var cached) && cached is string s) return s;

        var lang = ctx.Request.Query["lang"].ToString();
        if (!SupportedLangs.Contains(lang))
        {
            lang = ctx.Request.Cookies[CookieName] ?? "en";
        }
        if (!SupportedLangs.Contains(lang)) lang = "en";

        ctx.Items[ItemKey] = lang;
        return lang;
    }

    public static SiteStrings Strings(this HttpContext ctx) => SiteStrings.For(ctx.CurrentLang());

    // Generates a URL that switches to the given language, preserving the rest of the query string.

    public static string LangUrl(this HttpContext ctx, string targetLang)
    {
        var query = QueryHelpers.Replace(ctx.Request.QueryString.Value, "lang", targetLang);
        return ctx.Request.Path + query;
    }

    private static class QueryHelpers
    {
        public static string Replace(string? queryString, string key, string value)
        {
            var pairs = new List<string>();
            if (!string.IsNullOrEmpty(queryString))
            {
                foreach (var part in queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!part.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase)) pairs.Add(part);
                }
            }
            pairs.Add($"{key}={value}");
            return "?" + string.Join("&", pairs);
        }
    }
}

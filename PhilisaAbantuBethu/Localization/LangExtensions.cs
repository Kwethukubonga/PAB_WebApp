namespace PhilisaAbantuBethu.Localization;

public static class LangExtensions
{
    public const string CookieName = "pab_lang";
    private const string ItemKey = "pab_lang";

    /// <summary>Current language: ?lang= wins, then the cookie, then English.</summary>
    public static string CurrentLang(this HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(ItemKey, out var cached) && cached is string s) return s;

        var lang = ctx.Request.Query["lang"].ToString();
        if (lang != "en" && lang != "xh")
        {
            lang = ctx.Request.Cookies[CookieName] ?? "en";
        }
        if (lang != "en" && lang != "xh") lang = "en";

        ctx.Items[ItemKey] = lang;
        return lang;
    }

    public static SiteStrings Strings(this HttpContext ctx) => SiteStrings.For(ctx.CurrentLang());

    /// <summary>Link back to the current page in the other language.</summary>
    public static string LangToggleUrl(this HttpContext ctx)
    {
        var other = ctx.CurrentLang() == "en" ? "xh" : "en";
        var query = QueryHelpers.Replace(ctx.Request.QueryString.Value, "lang", other);
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
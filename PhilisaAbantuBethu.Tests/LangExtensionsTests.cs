using Microsoft.AspNetCore.Http;
using PhilisaAbantuBethu.Localization;

namespace PhilisaAbantuBethu.Tests;

public class LangExtensionsTests
{
    private static DefaultHttpContext ContextWithQuery(string? lang)
    {
        var ctx = new DefaultHttpContext();
        if (lang is not null) ctx.Request.QueryString = new QueryString($"?lang={lang}");
        return ctx;
    }

    [Fact]
    public void CurrentLang_DefaultsToEnglish_WhenNoQueryOrCookie()
    {
        var ctx = ContextWithQuery(null);

        Assert.Equal("en", ctx.CurrentLang());
    }

    [Theory]
    [InlineData("en")]
    [InlineData("xh")]
    [InlineData("af")]
    public void CurrentLang_ReturnsQueryStringLang_WhenValid(string lang)
    {
        var ctx = ContextWithQuery(lang);

        Assert.Equal(lang, ctx.CurrentLang());
    }

    [Fact]
    public void CurrentLang_FallsBackToCookie_WhenQueryStringMissing()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = $"{LangExtensions.CookieName}=af";

        Assert.Equal("af", ctx.CurrentLang());
    }

    [Fact]
    public void CurrentLang_DefaultsToEnglish_WhenQueryAndCookieBothInvalid()
    {
        var ctx = ContextWithQuery("fr");
        ctx.Request.Headers["Cookie"] = $"{LangExtensions.CookieName}=de";

        Assert.Equal("en", ctx.CurrentLang());
    }

    [Fact]
    public void CurrentLang_PrefersQueryStringOverCookie()
    {
        var ctx = ContextWithQuery("xh");
        ctx.Request.Headers["Cookie"] = $"{LangExtensions.CookieName}=af";

        Assert.Equal("xh", ctx.CurrentLang());
    }

    [Fact]
    public void LangUrl_SetsRequestedLanguage_AndKeepsOtherQueryParameters()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/Programmes/youth-programme";
        ctx.Request.QueryString = new QueryString("?lang=en&ref=nav");

        var url = ctx.LangUrl("af");

        Assert.StartsWith("/Programmes/youth-programme?", url);
        Assert.Contains("lang=af", url);
        Assert.Contains("ref=nav", url);
        Assert.DoesNotContain("lang=en", url);
    }

    [Fact]
    public void Strings_ReturnsSiteStringsMatchingCurrentLang()
    {
        var ctx = ContextWithQuery("xh");

        Assert.Equal("xh", ctx.Strings().Lang);
    }
}

using PhilisaAbantuBethu.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Remember the language chosen with ?lang=en / ?lang=xh
app.Use(async (context, next) =>
{
    var requested = context.Request.Query["lang"].ToString();
    if (requested is "en" or "xh" && context.Request.Cookies[LangExtensions.CookieName] != requested)
    {
        context.Response.Cookies.Append(LangExtensions.CookieName, requested, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            HttpOnly = false,
            SameSite = SameSiteMode.Lax
        });
    }
    await next();
});


app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

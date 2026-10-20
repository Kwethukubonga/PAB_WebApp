using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Security;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Everything under /Staff needs a signed-in PAB admin, apart from the sign-in page itself.
    options.Conventions.AuthorizeFolder("/Staff", StaffAuth.Policy);
    options.Conventions.AllowAnonymousToPage("/Staff/SignIn");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddSingleton<IReferenceGenerator, ReferenceGenerator>();

// Staff sign in with their Philisa Volunteers admin account (see Staff/).
builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.Section));
builder.Services.AddSingleton<IFirebaseTokenVerifier, FirebaseTokenVerifier>();
builder.Services.AddHttpClient<IStaffDirectory, FirestoreStaffDirectory>();

builder.Services.AddAuthentication(StaffAuth.Scheme)
    .AddCookie(StaffAuth.Scheme, options =>
    {
        options.LoginPath = "/Staff/SignIn";
        options.AccessDeniedPath = "/Staff/SignIn";
        options.Cookie.Name = "pab_staff";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy(StaffAuth.Policy, policy => policy
        .AddAuthenticationSchemes(StaffAuth.Scheme)
        .RequireClaim(StaffAuth.StaffClaim, "true")));

// Sign-in cookies and form tokens are encrypted with these keys. Keeping them on the Railway
// volume means a redeploy doesn't sign staff out or break a form someone has open.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("PhilisaAbantuBethu");
if (!string.IsNullOrWhiteSpace(keysPath))
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddFormRateLimiting(builder.Configuration);

// Railway's proxy handles HTTPS and passes the visitor's address on. Its address isn't fixed,
// so any proxy is trusted, but only the last hop it added is read, which a visitor can't fake.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Missing pages and refused form sends get a page that still shows how to reach PAB.
app.UseStatusCodePagesWithReExecute("/StatusCode", "?code={0}");

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Remember the language chosen with ?lang=en / ?lang=xh / ?lang=af
app.Use(async (context, next) =>
{
    var requested = context.Request.Query["lang"].ToString();
    if (requested is "en" or "xh" or "af" && context.Request.Cookies[LangExtensions.CookieName] != requested)
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

// Railway checks this before sending visitors to a new deploy. 200 when the database answers, 503 when not.
app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Lets the integration tests start the real app.
public partial class Program { }

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PhilisaAbantuBethu.Api;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Data.Protection;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Security;
using PhilisaAbantuBethu.Services;
using PhilisaAbantuBethu.Staff;

var builder = WebApplication.CreateBuilder(args);

// Nothing on this site legitimately sends more than a small form: there are no file uploads, the
// photos are built in rather than posted. Kestrel's 30MB default would let someone force a 30MB
// allocation per request against the public forms and the API, so the ceiling is dropped to
// 256KB for every request. The two public forms narrow it further to 64KB with [RequestSizeLimit].
builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = 256 * 1024);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Everything under /Staff needs a signed-in PAB admin, apart from the sign-in page itself.
    options.Conventions.AuthorizeFolder("/Staff", StaffAuth.Policy);
    options.Conventions.AllowAnonymousToPage("/Staff/SignIn");
});

// SQLite in development, Postgres once a managed database is set (C1). Which one is used depends
// only on the connection string, so moving across is an environment variable and a redeploy.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var usePostgres = DatabaseProvider.IsPostgres(connectionString);
if (usePostgres)
{
    // Everything asks for AppDbContext; on Postgres it is handed the Postgres version of it,
    // which has its own migrations and the table the data protection keys live in.
    builder.Services.AddDbContext<AppDbContext, PostgresAppDbContext>(options => options
        .UseNpgsql(DatabaseProvider.ToNpgsql(connectionString), npgsql =>
            // The managed database sleeps when nobody has used it for a few minutes, and the first
            // connection after that can fail while it wakes. Trying again is all it needs.
            npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null))
        .ReplaceService<IModelCacheKeyFactory, SensitiveModelCacheKeyFactory>());

    // The data protection key store asks for the Postgres context by its own type.
    builder.Services.AddScoped(services => (PostgresAppDbContext)services.GetRequiredService<AppDbContext>());
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options
        .UseSqlite(connectionString)
        .ReplaceService<IModelCacheKeyFactory, SensitiveModelCacheKeyFactory>());
}

builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IRetentionService, RetentionService>();
builder.Services.AddSingleton<IReferenceGenerator, ReferenceGenerator>();

// PAB's inbox hears about each new support request, without any personal details (W10).
builder.Services.Configure<AlertOptions>(builder.Configuration.GetSection(AlertOptions.Section));
builder.Services.AddHttpClient<IEmailSender, ResendEmailSender>(http =>
{
    http.BaseAddress = new Uri("https://api.resend.com/");
    http.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IStaffAlerts, StaffAlerts>();

// What someone writes about their situation is encrypted in the database (A5).
builder.Services.AddSingleton<ISensitiveText, SensitiveText>();

// Closed requests and old messages are deleted once they are past PAB's retention period (A5).
builder.Services.Configure<RetentionOptions>(builder.Configuration.GetSection(RetentionOptions.Section));
builder.Services.AddHostedService<RetentionWorker>();

// Staff sign in with their Philisa Volunteers admin account (see Staff/).
builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.Section));
builder.Services.AddSingleton<IFirebaseTokenVerifier, FirebaseTokenVerifier>();
builder.Services.AddHttpClient<IStaffDirectory, FirestoreStaffDirectory>();
builder.Services.AddHttpContextAccessor();

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
    })
    // The API takes the Firebase ID token the Android app already holds, so an admin's phone can
    // call it without signing in again (W1, B3).
    .AddScheme<AuthenticationSchemeOptions, StaffBearerHandler>(StaffApiAuth.Scheme, null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(StaffAuth.Policy, policy => policy
        .AddAuthenticationSchemes(StaffAuth.Scheme)
        .RequireClaim(StaffAuth.StaffClaim, "true"));

    // The staff endpoints take either: the staff pages' own cookie, or a bearer token whose
    // owner is an admin in Firestore. Signed out is 401; signed in without the role is 403.
    options.AddPolicy(StaffApiAuth.Policy, policy => policy
        .AddAuthenticationSchemes(StaffApiAuth.Scheme, StaffAuth.Scheme)
        .RequireAuthenticatedUser()
        .AddRequirements(new StaffRoleRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, StaffRoleHandler>();

// Problem details are the one shape every API error comes back in (W2).
builder.Services.AddProblemDetails();

// Publishes the API's description at /openapi/v1.json (B1).
builder.Services.AddOpenApi();

// Sign-in cookies, form tokens and the situation text are protected with these keys. Keeping them
// somewhere that survives a restart means a redeploy doesn't sign staff out, break a form someone
// has open or make saved requests unreadable. On Postgres they are in the database, encrypted with
// a certificate from the host's secret settings; otherwise they are files on the volume.
builder.Services.AddSiteDataProtection(builder.Configuration, builder.Environment, keysInDatabase: usePostgres);

builder.Services.AddFormRateLimiting(builder.Configuration);

// The host's proxy handles HTTPS and passes the visitor's address on. Its address isn't fixed,
// so any proxy is trusted, but only the entries the host's own proxies added are read.
//
// Trusting a forwarded header from any peer is only safe when there really is a proxy in front
// that sets it to the true client address. Reached without that proxy - run straight from the
// Docker image, locally, or on any host where a request can hit the container directly - the header
// is attacker-controlled, and since the rate limiter partitions on the client address, rotating it
// bypasses the limit on the forms entirely.
//
// So forwarded headers are trusted only when TrustProxy is set, which belongs on the hosted site
// (a host setting) and nowhere else. Without it the un-spoofable connection address is used, which
// is correct there.
//
// Where the visitor's address sits is host-specific, so two settings say where to look:
//  - TrustProxyHeader names a header that holds the visitor alone, if the host sets one;
//    left empty, the standard X-Forwarded-For is read.
//  - TrustProxyHops is where the visitor sits in X-Forwarded-For, counting from the right: 1 is
//    the last entry, 3 is the third from the right. Whatever is to the right of it was added by
//    the host's own proxies. Anything the visitor wrote themselves lands further left, so it
//    can't move the entry that is read. The default of 1 suits a host whose proxy adds the
//    visitor as the last entry; Render needs 3.
var trustProxy = builder.Configuration.GetValue<bool>("TrustProxy");
var trustProxyHeader = builder.Configuration["TrustProxyHeader"];
var trustProxyHops = Math.Max(1, builder.Configuration.GetValue("TrustProxyHops", 1));
if (trustProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        if (!string.IsNullOrWhiteSpace(trustProxyHeader))
        {
            options.ForwardedForHeaderName = trustProxyHeader;
        }
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        options.ForwardLimit = trustProxyHops;
    });
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // In development only, fill an empty database with something to look at (W9).
    if (app.Environment.IsDevelopment())
    {
        await DevelopmentSeed.RunAsync(scope.ServiceProvider);
    }

}

// Not a way of running the site: after a backup is restored, this opens every saved request to prove
// the backup and the certificate still work together, prints counts, and stops.
if (args.Contains(DecryptionCheck.Argument))
{
    Environment.ExitCode = await DecryptionCheck.RunAsync(app.Services, Console.Out);
    return;
}

// Configure the HTTP request pipeline.
// Only when there is a trusted proxy in front of the site. See the ForwardedHeaders note above.
if (trustProxy)
{
    app.UseForwardedHeaders();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Inside the page handler above, so an /api error comes back as JSON rather than HTML (W2).
app.UseApiErrors();

// A refusal with no body of its own - a 401, 403 or 404 - still has to say something. An API
// caller gets it as problem details; a visitor gets a page that shows how to reach PAB.
app.UseWhen(context => ApiProblem.IsApiPath(context.Request.Path),
    api => api.UseStatusCodePages(ApiProblem.WriteStatusAsync));
app.UseWhen(context => !ApiProblem.IsApiPath(context.Request.Path),
    pages => pages.UseStatusCodePagesWithReExecute("/StatusCode", "?code={0}"));

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();

// Method, path, status and time for every request, with nothing personal in it (W7).
app.UseRequestLogging();

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

// 200 when the database answers, 503 when not. For checking the site by hand and after a deploy.
// Asking it often keeps the managed database awake, which uses up its free monthly hours.
app.MapHealthChecks("/health");

// 200 as long as the site itself is running, and never touches the database. This is the address
// for the host's health check and the uptime monitor, which ask every few minutes: the database can
// sleep between real visitors. A deploy whose database can't be reached never gets this far,
// because the migrations at start-up fail first.
app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = _ => false });

// One group per resource, each one calling the same services the pages do (B1, B2, W6).
app.MapSupportRequests();
app.MapContact();
app.MapContent();

app.MapOpenApi();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Lets the integration tests start the real app.
public partial class Program { }

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Pages;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

public class RequestSupportModelTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static RequestSupportModel CreateModel(AppDbContext db) =>
        new(new SupportRequestService(db, new ReferenceGenerator(), new NoAlerts(), NullLogger<SupportRequestService>.Instance), new AcceptingAntiforgery())
        {
            PageContext = PageModelTestHelpers.CreatePageContext(),
            TempData = PageModelTestHelpers.CreateTempData(),
            FirstName = "Nomsa",
            Surname = "Dlamini",
            Phone = "071 123 4567",
            Email = "nomsa@example.com",
            Area = "Lavender Hill",
            ContactMethod = "phone",
            SupportType = "Food Assistance",
            Situation = "Need help with groceries this month.",
            Urgent = "no",
            Extra = "",
            Declaration = true
        };

    [Fact]
    public async Task OnPostAsync_SavesSupportRequest_WhenValid()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);

        await model.OnPostAsync();

        var saved = Assert.Single(db.SupportRequests);
        Assert.Equal("Nomsa", saved.FirstName);
        Assert.Equal("Dlamini", saved.Surname);
        Assert.Equal("Lavender Hill", saved.Area);
        Assert.True(saved.Declaration);
        Assert.Equal("new", saved.Status);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToConfirmation_WithGeneratedReference()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Confirmation", redirect.PageName);
        // The reference goes through TempData, never the address (issue #24).
        Assert.Null(redirect.RouteValues);
        var reference = Assert.IsType<string>(model.TempData[ConfirmationModel.ReferenceKey]);
        Assert.Matches("^PAB-[2-9A-Z]{6}$", reference);
        Assert.Equal("phone", model.TempData[ConfirmationModel.ContactMethodKey]);
    }

    [Fact]
    public async Task OnPostAsync_DoesNotSave_WhenModelStateInvalid()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);
        model.ModelState.AddModelError("FirstName", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(db.SupportRequests);
    }

    [Fact]
    public async Task OnPostAsync_ShowsFieldErrors_AndSavesNothing_WhenTheServerChecksFail()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);
        model.FirstName = "";
        model.Phone = "0888888888";
        model.Declaration = false;

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(db.SupportRequests);
        Assert.NotNull(model.ErrorFor("FirstName"));
        Assert.NotNull(model.ErrorFor("Phone"));
        Assert.NotNull(model.ErrorFor("Declaration"));
        Assert.Null(model.ErrorFor("Area"));
        Assert.NotNull(model.FormError);
    }

    [Fact]
    public async Task OnPostAsync_SavesNothing_WhenTheBotTrapIsFilledIn()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);
        model.Website = "http://spam.example";

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(db.SupportRequests);
        Assert.NotNull(model.FormError);
    }
}

/// <summary>The date on the confirmation page (#27).</summary>
public class ConfirmationDateTests
{
    [Fact]
    public void JustAfterMidnight_InSouthAfrica_ShowsTodayNotYesterday()
    {
        // 23:30 UTC on 26 October is 01:30 on 27 October in South Africa.
        var utc = new DateTime(2026, 10, 26, 23, 30, 0, DateTimeKind.Utc);

        Assert.Equal("27 October 2026", ConfirmationModel.DateFor(utc, "en"));
    }

    [Fact]
    public void TheMonth_IsInTheChosenLanguage()
    {
        var utc = new DateTime(2026, 10, 27, 9, 0, 0, DateTimeKind.Utc);

        Assert.Equal("27 Oktober 2026", ConfirmationModel.DateFor(utc, "af"));
        Assert.DoesNotContain("October", ConfirmationModel.DateFor(utc, "xh"));
    }
}

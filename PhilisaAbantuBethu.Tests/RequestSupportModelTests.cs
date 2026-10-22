using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Pages;

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

    private static RequestSupportModel CreateModel(AppDbContext db) => new(db)
    {
        PageContext = PageModelTestHelpers.CreatePageContext(),
        FirstName = "Nomsa",
        Surname = "Dlamini",
        Phone = "0710000000",
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
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToConfirmation_WithGeneratedReference()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Confirmation", redirect.PageName);
        var reference = Assert.IsType<string>(
            redirect.RouteValues!["reference"]);
        Assert.StartsWith("PAB", reference);
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
}

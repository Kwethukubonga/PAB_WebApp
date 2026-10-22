using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Pages;

namespace PhilisaAbantuBethu.Tests;

public class ContactModelTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ContactModel CreateModel(AppDbContext db) => new(db)
    {
        PageContext = PageModelTestHelpers.CreatePageContext(),
        Name = "Nomsa Dlamini",
        Email = "nomsa@example.com",
        Phone = "0710000000",
        Subject = "General Enquiry",
        Message = "Hello, I'd like more information."
    };

    [Fact]
    public async Task OnPostAsync_SavesContactInquiry_WhenValid()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);

        await model.OnPostAsync();

        var saved = Assert.Single(db.ContactInquiries);
        Assert.Equal("Nomsa Dlamini", saved.Name);
        Assert.Equal("General Enquiry", saved.Subject);
    }

    [Fact]
    public async Task OnPostAsync_RedirectsWithSentTrue_WhenValid()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.True((bool)redirect.RouteValues!["sent"]!);
    }

    [Fact]
    public async Task OnPostAsync_DoesNotSave_WhenModelStateInvalid()
    {
        using var db = CreateInMemoryDb();
        var model = CreateModel(db);
        model.ModelState.AddModelError("Name", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(db.ContactInquiries);
    }
}

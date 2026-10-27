using PhilisaAbantuBethu.Data;

namespace PhilisaAbantuBethu.Tests;

public class SiteDataTests
{
    [Fact]
    public void FindProgramme_ReturnsMatchingProgramme_ForValidId()
    {
        var prog = SiteData.FindProgramme("baby-saver")!;

        Assert.Equal("baby-saver", prog.Id);
        Assert.Equal("Baby Saver", prog.Title);
    }

    // No fallback to the first programme: the page answers 404 instead (#25).
    [Theory]
    [InlineData("does-not-exist")]
    [InlineData("<script>")]
    [InlineData("")]
    [InlineData(null)]
    public void FindProgramme_FindsNothing_ForAnUnknownId(string? id)
    {
        Assert.Null(SiteData.FindProgramme(id));
    }

    [Fact]
    public void Programmes_HasTenEntries()
    {
        Assert.Equal(10, SiteData.Programmes.Count);
    }

    [Fact]
    public void Programmes_AllHaveUniqueIds()
    {
        var ids = SiteData.Programmes.Select(p => p.Id).ToList();

        Assert.Equal(ids.Distinct().Count(), ids.Count);
    }

    [Fact]
    public void BabySaver_HasEmergencyContactInfo()
    {
        var prog = SiteData.FindProgramme("baby-saver")!;

        Assert.False(string.IsNullOrEmpty(prog.EmergencyAddress));
        Assert.False(string.IsNullOrEmpty(prog.EmergencyPhone));
    }
}

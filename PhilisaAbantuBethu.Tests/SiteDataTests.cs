using PhilisaAbantuBethu.Data;

namespace PhilisaAbantuBethu.Tests;

public class SiteDataTests
{
    [Fact]
    public void FindProgramme_ReturnsMatchingProgramme_ForValidId()
    {
        var prog = SiteData.FindProgramme("baby-saver");

        Assert.Equal("baby-saver", prog.Id);
        Assert.Equal("Baby Saver", prog.Title);
    }

    [Fact]
    public void FindProgramme_FallsBackToFirstProgramme_ForUnknownId()
    {
        var prog = SiteData.FindProgramme("does-not-exist");

        Assert.Equal(SiteData.Programmes[0].Id, prog.Id);
    }

    [Fact]
    public void FindProgramme_FallsBackToFirstProgramme_ForNullId()
    {
        var prog = SiteData.FindProgramme(null);

        Assert.Equal(SiteData.Programmes[0].Id, prog.Id);
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
        var prog = SiteData.FindProgramme("baby-saver");

        Assert.False(string.IsNullOrEmpty(prog.EmergencyAddress));
        Assert.False(string.IsNullOrEmpty(prog.EmergencyPhone));
    }
}

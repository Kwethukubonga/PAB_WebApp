using PhilisaAbantuBethu.Security;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// A log line must never be a way to read who asked for what (W7). The path is reduced to its
/// shape, so one person's request can't be picked out of the logs by its id or reference.
/// </summary>
public class RequestLoggingTests
{
    [Theory]
    [InlineData("/api/support-requests/41", "/api/support-requests/{id}")]
    [InlineData("/api/support-requests/by-reference/PAB-7KQ39M", "/api/support-requests/by-reference/{id}")]
    [InlineData("/Staff/Requests/Details/128", "/Staff/Requests/Details/{id}")]
    [InlineData("/api/support-requests", "/api/support-requests")]
    [InlineData("/RequestSupport", "/RequestSupport")]
    [InlineData("/", "/")]
    public void An_identifying_path_is_logged_as_its_shape(string path, string expected)
    {
        Assert.Equal(expected, RequestLoggingMiddleware.Shape(path));
    }

    [Fact]
    public void A_programme_name_is_not_mistaken_for_an_id()
    {
        // Only numbers and references are replaced: a public page keeps its name.
        Assert.Equal("/api/programmes/womens-empowerment", RequestLoggingMiddleware.Shape("/api/programmes/womens-empowerment"));
    }
}

namespace PhilisaAbantuBethu.Data;

/// <summary>
/// A photo as a page asks for it. Passed to the _Image partial, which turns it into a
/// &lt;picture&gt; with the WebP copies and the photo's own width and height (H2).
/// </summary>
/// <param name="Src">The original path, as it appears in SiteData, for example /img/home/hero.jpg.</param>
/// <param name="Alt">
/// What the photo says, for somebody who cannot see it. Empty for a photo that is only decoration,
/// which should also set <paramref name="Decorative"/>.
/// </param>
/// <param name="Class">The CSS classes for the img itself.</param>
/// <param name="Sizes">
/// How wide the photo is shown, so the browser can pick a width before the stylesheet has been
/// worked out. The default assumes it fills the screen on a phone and a third of it on a laptop.
/// </param>
/// <param name="Eager">
/// True for a photo above the first screen, which should be fetched straight away. Everything
/// else waits until it is nearly in view.
/// </param>
/// <param name="Decorative">True when the photo adds nothing a reader needs, so it is hidden from screen readers.</param>
/// <param name="Style">Any inline style the layout needs, such as a hero's focal point.</param>
public record SitePhoto(
    string Src,
    string Alt = "",
    string Class = "",
    string Sizes = "(min-width: 1024px) 33vw, 100vw",
    bool Eager = false,
    bool Decorative = false,
    string? Style = null);

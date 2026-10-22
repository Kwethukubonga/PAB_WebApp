// Written by tools/images/optimise.js. Do not edit by hand; run `npm run images`.

namespace PhilisaAbantuBethu.Data;

/// <summary>One WebP copy of a photo, at one width.</summary>
public readonly record struct ImageVariant(int Width, string Url);

/// <summary>A photo's real shape, and the WebP copies made from it.</summary>
public readonly record struct ImageInfo(int Width, int Height, ImageVariant[] Variants)
{
    /// <summary>The srcset a browser picks from, widest last.</summary>
    public string SrcSet => string.Join(", ", Variants.Select(v => $"{v.Url} {v.Width}w"));
}

/// <summary>
/// Every photo on the site, with its shape and its WebP copies (H2). Used by _Image.cshtml so a
/// page reserves the right space before a photo arrives and never jumps as it loads, and so the
/// browser downloads the width it will actually show rather than the full-size original.
/// </summary>
public static class SiteImages
{
    private static readonly Dictionary<string, ImageInfo> All = new()
    {
        ["/img/about/history-1.jpg"] = new(750, 650, [new(400, "/img/about/history-1-400.webp")]),
        ["/img/about/history-2.jpg"] = new(1024, 683, [new(400, "/img/about/history-2-400.webp"), new(900, "/img/about/history-2-900.webp")]),
        ["/img/about/history-3.jpg"] = new(750, 650, [new(400, "/img/about/history-3-400.webp")]),
        ["/img/about/history-4.jpg"] = new(750, 650, [new(400, "/img/about/history-4-400.webp")]),
        ["/img/brand/logo.png"] = new(256, 261, [new(256, "/img/brand/logo-256.webp")]),
        ["/img/home/hero.jpg"] = new(1600, 1067, [new(400, "/img/home/hero-400.webp"), new(900, "/img/home/hero-900.webp"), new(1600, "/img/home/hero-1600.webp")]),
        ["/img/programmes/after-school/gallery-1.jpg"] = new(1600, 1067, [new(400, "/img/programmes/after-school/gallery-1-400.webp"), new(900, "/img/programmes/after-school/gallery-1-900.webp"), new(1600, "/img/programmes/after-school/gallery-1-1600.webp")]),
        ["/img/programmes/after-school/gallery-2.jpg"] = new(1600, 1067, [new(400, "/img/programmes/after-school/gallery-2-400.webp"), new(900, "/img/programmes/after-school/gallery-2-900.webp"), new(1600, "/img/programmes/after-school/gallery-2-1600.webp")]),
        ["/img/programmes/after-school/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/after-school/gallery-3-400.webp")]),
        ["/img/programmes/after-school/gallery-4.jpg"] = new(1024, 683, [new(400, "/img/programmes/after-school/gallery-4-400.webp"), new(900, "/img/programmes/after-school/gallery-4-900.webp")]),
        ["/img/programmes/after-school/hero.jpg"] = new(1600, 1067, [new(400, "/img/programmes/after-school/hero-400.webp"), new(900, "/img/programmes/after-school/hero-900.webp"), new(1600, "/img/programmes/after-school/hero-1600.webp")]),
        ["/img/programmes/baby-saver/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/baby-saver/gallery-1-400.webp")]),
        ["/img/programmes/baby-saver/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/baby-saver/gallery-2-400.webp")]),
        ["/img/programmes/baby-saver/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/baby-saver/hero-400.webp")]),
        ["/img/programmes/community-feeding/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/community-feeding/gallery-1-400.webp")]),
        ["/img/programmes/community-feeding/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/community-feeding/gallery-2-400.webp")]),
        ["/img/programmes/community-feeding/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/community-feeding/gallery-3-400.webp")]),
        ["/img/programmes/community-feeding/gallery-4.jpg"] = new(750, 650, [new(400, "/img/programmes/community-feeding/gallery-4-400.webp")]),
        ["/img/programmes/community-feeding/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/community-feeding/hero-400.webp")]),
        ["/img/programmes/mens-cafe/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/mens-cafe/gallery-1-400.webp")]),
        ["/img/programmes/mens-cafe/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/mens-cafe/gallery-2-400.webp")]),
        ["/img/programmes/mens-cafe/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/mens-cafe/gallery-3-400.webp")]),
        ["/img/programmes/mens-cafe/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/mens-cafe/hero-400.webp")]),
        ["/img/programmes/safe-houses/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/safe-houses/gallery-1-400.webp")]),
        ["/img/programmes/safe-houses/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/safe-houses/gallery-2-400.webp")]),
        ["/img/programmes/safe-houses/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/safe-houses/gallery-3-400.webp")]),
        ["/img/programmes/safe-houses/gallery-4.jpg"] = new(750, 650, [new(400, "/img/programmes/safe-houses/gallery-4-400.webp")]),
        ["/img/programmes/safe-houses/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/safe-houses/hero-400.webp")]),
        ["/img/programmes/search-rescue/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/search-rescue/gallery-1-400.webp")]),
        ["/img/programmes/search-rescue/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/search-rescue/hero-400.webp")]),
        ["/img/programmes/senior-programme/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/senior-programme/gallery-1-400.webp")]),
        ["/img/programmes/senior-programme/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/senior-programme/gallery-2-400.webp")]),
        ["/img/programmes/senior-programme/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/senior-programme/gallery-3-400.webp")]),
        ["/img/programmes/senior-programme/gallery-4.jpg"] = new(750, 650, [new(400, "/img/programmes/senior-programme/gallery-4-400.webp")]),
        ["/img/programmes/senior-programme/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/senior-programme/hero-400.webp")]),
        ["/img/programmes/social-work/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/social-work/gallery-1-400.webp")]),
        ["/img/programmes/social-work/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/social-work/gallery-2-400.webp")]),
        ["/img/programmes/social-work/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/social-work/gallery-3-400.webp")]),
        ["/img/programmes/social-work/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/social-work/hero-400.webp")]),
        ["/img/programmes/womens-empowerment/gallery-1.jpg"] = new(1600, 1067, [new(400, "/img/programmes/womens-empowerment/gallery-1-400.webp"), new(900, "/img/programmes/womens-empowerment/gallery-1-900.webp"), new(1600, "/img/programmes/womens-empowerment/gallery-1-1600.webp")]),
        ["/img/programmes/womens-empowerment/gallery-2.jpg"] = new(1600, 1067, [new(400, "/img/programmes/womens-empowerment/gallery-2-400.webp"), new(900, "/img/programmes/womens-empowerment/gallery-2-900.webp"), new(1600, "/img/programmes/womens-empowerment/gallery-2-1600.webp")]),
        ["/img/programmes/womens-empowerment/gallery-3.jpg"] = new(1600, 1067, [new(400, "/img/programmes/womens-empowerment/gallery-3-400.webp"), new(900, "/img/programmes/womens-empowerment/gallery-3-900.webp"), new(1600, "/img/programmes/womens-empowerment/gallery-3-1600.webp")]),
        ["/img/programmes/womens-empowerment/gallery-4.jpg"] = new(1600, 1067, [new(400, "/img/programmes/womens-empowerment/gallery-4-400.webp"), new(900, "/img/programmes/womens-empowerment/gallery-4-900.webp"), new(1600, "/img/programmes/womens-empowerment/gallery-4-1600.webp")]),
        ["/img/programmes/womens-empowerment/hero.jpg"] = new(1600, 1067, [new(400, "/img/programmes/womens-empowerment/hero-400.webp"), new(900, "/img/programmes/womens-empowerment/hero-900.webp"), new(1600, "/img/programmes/womens-empowerment/hero-1600.webp")]),
        ["/img/programmes/youth-programme/gallery-1.jpg"] = new(750, 650, [new(400, "/img/programmes/youth-programme/gallery-1-400.webp")]),
        ["/img/programmes/youth-programme/gallery-2.jpg"] = new(750, 650, [new(400, "/img/programmes/youth-programme/gallery-2-400.webp")]),
        ["/img/programmes/youth-programme/gallery-3.jpg"] = new(750, 650, [new(400, "/img/programmes/youth-programme/gallery-3-400.webp")]),
        ["/img/programmes/youth-programme/gallery-4.jpg"] = new(750, 650, [new(400, "/img/programmes/youth-programme/gallery-4-400.webp")]),
        ["/img/programmes/youth-programme/hero.jpg"] = new(750, 650, [new(400, "/img/programmes/youth-programme/hero-400.webp")]),
        ["/img/team/alinafe-kamtolo.jpg"] = new(800, 800, [new(400, "/img/team/alinafe-kamtolo-400.webp")]),
        ["/img/team/brandon-samuels.jpg"] = new(800, 800, [new(400, "/img/team/brandon-samuels-400.webp")]),
        ["/img/team/bright-kamtolo.jpg"] = new(800, 800, [new(400, "/img/team/bright-kamtolo-400.webp")]),
        ["/img/team/esmeralda-greeff.jpg"] = new(800, 800, [new(400, "/img/team/esmeralda-greeff-400.webp")]),
        ["/img/team/francina-esbach.jpg"] = new(800, 800, [new(400, "/img/team/francina-esbach-400.webp")]),
        ["/img/team/gail-van-rensburg.jpg"] = new(800, 800, [new(400, "/img/team/gail-van-rensburg-400.webp")]),
        ["/img/team/gavin-peters.jpg"] = new(800, 800, [new(400, "/img/team/gavin-peters-400.webp")]),
        ["/img/team/janine-poggenpoel.jpg"] = new(800, 800, [new(400, "/img/team/janine-poggenpoel-400.webp")]),
        ["/img/team/lena-rayepen.jpg"] = new(800, 800, [new(400, "/img/team/lena-rayepen-400.webp")]),
        ["/img/team/lindsay-adams.jpg"] = new(800, 800, [new(400, "/img/team/lindsay-adams-400.webp")]),
        ["/img/team/lucinda-evans.jpg"] = new(800, 800, [new(400, "/img/team/lucinda-evans-400.webp")]),
        ["/img/team/melissa-rudolph.jpg"] = new(800, 800, [new(400, "/img/team/melissa-rudolph-400.webp")]),
        ["/img/team/nthombikayise-ndwandwa.jpg"] = new(800, 800, [new(400, "/img/team/nthombikayise-ndwandwa-400.webp")]),
        ["/img/team/nthombizokwanda-sihulu.jpg"] = new(800, 800, [new(400, "/img/team/nthombizokwanda-sihulu-400.webp")]),
        ["/img/team/olivia-maart.jpg"] = new(800, 800, [new(400, "/img/team/olivia-maart-400.webp")]),
        ["/img/team/rihanna-galant.jpg"] = new(800, 800, [new(400, "/img/team/rihanna-galant-400.webp")]),
        ["/img/team/roelien-pimentel.jpg"] = new(800, 800, [new(400, "/img/team/roelien-pimentel-400.webp")]),
        ["/img/team/stacey-samuels.jpg"] = new(800, 800, [new(400, "/img/team/stacey-samuels-400.webp")]),
        ["/img/team/sylvia-kruger.jpg"] = new(800, 800, [new(400, "/img/team/sylvia-kruger-400.webp")]),
        ["/img/team/yvonne-botha.jpg"] = new(800, 800, [new(400, "/img/team/yvonne-botha-400.webp")]),
        ["/img/team/zimvo-katsala.jpg"] = new(800, 800, [new(400, "/img/team/zimvo-katsala-400.webp")]),
        ["/img/testimonials/anna.jpg"] = new(300, 300, [new(300, "/img/testimonials/anna-300.webp")]),
        ["/img/testimonials/carley.jpg"] = new(300, 300, [new(300, "/img/testimonials/carley-300.webp")]),
        ["/img/testimonials/ira.jpg"] = new(300, 300, [new(300, "/img/testimonials/ira-300.webp")]),
        ["/img/testimonials/michelle.jpg"] = new(300, 300, [new(300, "/img/testimonials/michelle-300.webp")]),
        ["/img/testimonials/sarah.jpg"] = new(300, 300, [new(300, "/img/testimonials/sarah-300.webp")]),
        ["/img/testimonials/thea.jpg"] = new(300, 300, [new(300, "/img/testimonials/thea-300.webp")]),
    };

    /// <summary>What is known about this photo, or null for one that was never converted.</summary>
    public static ImageInfo? For(string? url) =>
        url is not null && All.TryGetValue(url, out var info) ? info : null;

    public static int Count => All.Count;
}

#!/usr/bin/env node
/**
 * Writes a WebP copy of every photo, and records each one's size (H2).
 *
 *   node tools/images/optimise.js
 *
 * PAB's community buys data by the day. The photos were full-size JPEGs: 7.5MB in all, with the
 * home page alone pulling over a megabyte before anybody had read a word. WebP at quality 78 is
 * about a third of that at the size these are actually shown.
 *
 * The JPEGs stay. _Image.cshtml serves the WebP through a <picture> and falls back to the JPEG,
 * so a browser that cannot read WebP still sees the photo.
 *
 * It also writes design/image-sizes.json, which gives every photo its real width and height so
 * the pages can reserve the space and stop the layout jumping as they load.
 */

const fs = require('node:fs');
const path = require('node:path');
const sharp = require('sharp');

const root = path.resolve(__dirname, '..', '..');
const images = path.join(root, 'PhilisaAbantuBethu', 'wwwroot', 'img');

/*
 * Three widths, because the same photo is used at very different sizes: 400 for a card or an
 * avatar, 900 for a gallery tile on a phone, 1600 for a hero on a laptop. A team avatar is shown
 * at 80px and was a 1200px JPEG, which is most of the waste on the About page. The browser picks
 * from srcset; a photo already narrower than a width simply doesn't get that one.
 */
const WIDTHS = [400, 900, 1600];
const QUALITY = 78;

function photos(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) return photos(full);
    return /\.(jpe?g|png)$/i.test(entry.name) ? [full] : [];
  });
}

(async () => {
  const sizes = {};
  let before = 0;
  let after = 0;

  for (const file of photos(images)) {
    const original = fs.statSync(file).size;
    before += original;

    const meta = await sharp(file).metadata();
    const url = '/img/' + path.relative(images, file).split(path.sep).join('/');

    // Only widths the photo can actually fill, plus its own width when it is smaller than all
    // of them, so every photo has at least one WebP.
    let wanted = WIDTHS.filter((w) => w <= meta.width);
    if (wanted.length === 0) wanted = [meta.width];

    const variants = [];
    for (const width of wanted) {
      const webp = file.replace(/\.(jpe?g|png)$/i, `-${width}.webp`);
      await sharp(file)
        .resize({ width, withoutEnlargement: true })
        .webp({ quality: QUALITY })
        .toFile(webp);

      const size = fs.statSync(webp).size;
      after += size;
      variants.push({ width, url: url.replace(/\.(jpe?g|png)$/i, `-${width}.webp`) });
    }

    sizes[url] = {
      // The photo's own shape, for the width and height the page reserves.
      width: meta.width,
      height: meta.height,
      variants
    };

    const biggest = fs.statSync(file.replace(/\.(jpe?g|png)$/i, `-${wanted[wanted.length - 1]}.webp`)).size;
    console.log(
      `${url.padEnd(52)} ${(original / 1024).toFixed(0).padStart(5)}KB -> ` +
        `${(biggest / 1024).toFixed(0).padStart(5)}KB at ${wanted[wanted.length - 1]}w ` +
        `(${wanted.length} width${wanted.length === 1 ? '' : 's'})`
    );
  }

  fs.writeFileSync(
    path.join(root, 'design', 'image-sizes.json'),
    JSON.stringify(sizes, null, 1) + '\n'
  );

  // Also as C#, so the pages can reserve each photo's space and build its srcset without
  // reading a file at runtime, and a photo that was never converted fails the build rather
  // than quietly shipping as a full-size JPEG.
  const entries = Object.entries(sizes)
    .map(([url, info]) => {
      const variants = info.variants
        .map((v) => `new(${v.width}, "${v.url}")`)
        .join(', ');
      return `        ["${url}"] = new(${info.width}, ${info.height}, [${variants}]),`;
    })
    .join('\n');

  const cs = `// Written by tools/images/optimise.js. Do not edit by hand; run \`npm run images\`.

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
${entries}
    };

    /// <summary>What is known about this photo, or null for one that was never converted.</summary>
    public static ImageInfo? For(string? url) =>
        url is not null && All.TryGetValue(url, out var info) ? info : null;

    public static int Count => All.Count;
}
`;

  fs.writeFileSync(path.join(root, 'PhilisaAbantuBethu', 'Data', 'SiteImages.g.cs'), cs);

  console.log(
    `\n${(before / 1024 / 1024).toFixed(1)}MB of photos -> ${(after / 1024 / 1024).toFixed(1)}MB as WebP ` +
      `(${(100 - (after / before) * 100).toFixed(0)}% smaller)`
  );
})();

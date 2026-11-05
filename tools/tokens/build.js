#!/usr/bin/env node
/**
 * Writes the design tokens out to each app, so one change reaches both (E1).
 *
 *   node tools/tokens/build.js                       writes the website's Tailwind theme
 *   node tools/tokens/build.js --android <repo path>  also writes Android's colours and dimens
 *
 * Nothing it writes should be edited by hand: change design/tokens.json and run it again.
 */

const fs = require('node:fs');
const path = require('node:path');

const root = path.resolve(__dirname, '..', '..');
const tokens = JSON.parse(fs.readFileSync(path.join(root, 'design', 'tokens.json'), 'utf8'));

const GENERATED = 'Written by tools/tokens/build.js from design/tokens.json. Do not edit by hand.';

/** Drops the $comment keys that explain the tokens to a reader. */
function plain(object) {
  return Object.fromEntries(Object.entries(object).filter(([key]) => !key.startsWith('$')));
}

// ---------------------------------------------------------------- the website

function writeTailwindTheme() {
  const brand = plain(tokens.colour.brand);
  const neutral = plain(tokens.colour.neutral);
  const status = plain(tokens.colour.status);

  const theme = {
    colors: {
      brand,
      ink: neutral,
      cream: tokens.colour.surface.cream,
      status,
      'on-brand': plain(tokens.colour.onBrand)
    },
    fontFamily: {
      // Quoted: a family name whose last word is a number ("Source Sans 3") is not a valid
      // unquoted CSS identifier, and a browser throws the whole declaration away without it.
      serif: [`'${tokens.type.display}'`, 'Georgia', 'serif'],
      sans: [`'${tokens.type.body}'`, 'system-ui', 'sans-serif']
    },
    borderRadius: plain(tokens.radius),
    spacing: plain(tokens.space),
    minHeight: { target: tokens.target.web },
    minWidth: { target: tokens.target.web }
  };

  const file = path.join(root, 'tailwind.tokens.js');
  fs.writeFileSync(file, `// ${GENERATED}\nmodule.exports = ${JSON.stringify(theme, null, 2)};\n`);
  return file;
}

// ---------------------------------------------------------------- Android

function androidColours() {
  const brand = plain(tokens.colour.brand);
  const status = plain(tokens.colour.status);
  const neutral = plain(tokens.colour.neutral);

  const lines = [
    '<?xml version="1.0" encoding="utf-8"?>',
    `<!-- ${GENERATED} -->`,
    '<resources>',
    '',
    '    <!-- Brand purples (header band, primary actions) -->'
  ];

  for (const [step, value] of Object.entries(brand)) {
    lines.push(`    <color name="purple_${step}">${value.toUpperCase()}</color>`);
  }

  lines.push('', '    <!-- Purple for text and icons on cards -->');
  lines.push(`    <color name="accent">${brand['700'].toUpperCase()}</color>`);

  lines.push('', '    <!-- Surfaces -->');
  lines.push(`    <color name="background_cream">${tokens.colour.surface.cream.toUpperCase()}</color>`);
  lines.push(`    <color name="card_background">${tokens.colour.surface.card.toUpperCase()}</color>`);
  lines.push(`    <color name="divider">${neutral['200'].toUpperCase()}</color>`);
  lines.push(`    <color name="white">${tokens.colour.surface.white.toUpperCase()}</color>`);

  lines.push('', '    <!-- Text -->');
  lines.push(`    <color name="text_primary">${neutral['900'].toUpperCase()}</color>`);
  lines.push(`    <color name="text_secondary">${neutral['500'].toUpperCase()}</color>`);
  lines.push(`    <color name="text_on_purple">${tokens.colour.onBrand.default.toUpperCase()}</color>`);
  lines.push('    <!-- White at 85%: the old #D8B4FE measured 3.2:1 on the gradient (F9). -->');
  lines.push('    <color name="text_on_purple_muted">#D9FFFFFF</color>');
  lines.push(`    <color name="text_on_light">${neutral['900'].toUpperCase()}</color>`);

  lines.push('', '    <!-- Status pills: one colour per status, shared with the website -->');
  lines.push(`    <color name="status_success_bg">${status.successSurface.toUpperCase()}</color>`);
  lines.push(`    <color name="status_success_text">${status.success.toUpperCase()}</color>`);
  lines.push(`    <color name="status_error_text">${status.urgent.toUpperCase()}</color>`);
  lines.push(`    <color name="status_info_bg">${status.inProgressSurface.toUpperCase()}</color>`);
  lines.push(`    <color name="status_info_text">${status.inProgress.toUpperCase()}</color>`);
  lines.push(`    <color name="status_neutral_bg">${status.closedSurface.toUpperCase()}</color>`);
  lines.push(`    <color name="status_neutral_text">${status.closed.toUpperCase()}</color>`);

  lines.push('', '    <!-- Stat tiles -->');
  lines.push(`    <color name="tile_lavender">${brand['100'].toUpperCase()}</color>`);
  lines.push('    <!-- Drop shadow behind text sitting on a photo. -->');
  lines.push('    <color name="text_shadow">#99000000</color>');
  lines.push('</resources>', '');

  return lines.join('\n');
}

/**
 * Only the colours are written to Android. The mismatch the marker named was the palette, and
 * Android already has its own spacing and radius scale with its own names, in use across the
 * layouts; generating a second one beside it would be two names for the same 16dp, not one
 * source of truth. The touch target is checked rather than written, for the same reason.
 *
 * values-night/colors.xml is left alone: the dark palette is a real second scheme, chosen for
 * dark surfaces, not these colours inverted.
 */
function writeAndroid(repo) {
  const values = path.join(repo, 'app', 'src', 'main', 'res', 'values');
  if (!fs.existsSync(values)) {
    throw new Error(`No Android resources at ${values}. Pass the path to the PAB_Volunteers checkout.`);
  }

  const colours = path.join(values, 'colors.xml');
  fs.writeFileSync(colours, androidColours());

  // A warning, not a failure: it is a number in a file this script does not own.
  const dimens = path.join(values, 'dimens.xml');
  if (fs.existsSync(dimens)) {
    const found = /<dimen name="touch_target_min">([^<]+)<\/dimen>/.exec(fs.readFileSync(dimens, 'utf8'));
    if (found && found[1] !== tokens.target.android) {
      console.warn(
        `warning: touch_target_min is ${found[1]} in dimens.xml but ${tokens.target.android} in tokens.json`
      );
    }
  }

  return [colours];
}

// ---------------------------------------------------------------- run

const written = [writeTailwindTheme()];

const androidFlag = process.argv.indexOf('--android');
if (androidFlag !== -1) {
  const repo = process.argv[androidFlag + 1];
  if (!repo) {
    console.error('--android needs the path to the PAB_Volunteers checkout.');
    process.exit(1);
  }
  written.push(...writeAndroid(path.resolve(repo)));
}

for (const file of written) {
  console.log(`wrote ${path.relative(process.cwd(), file)}`);
}

const tokens = require('./tailwind.tokens');

/**
 * The website's Tailwind build (H1). Until now the site loaded cdn.tailwindcss.com, which
 * compiles every utility in the visitor's browser on every page load: slow, and not meant for
 * production. This outputs only the classes the pages actually use, into one small file.
 *
 * The theme comes from design/tokens.json through tailwind.tokens.js, so the brand colours and
 * type are the same here as on Android (E1). Run `npm run build` after changing the tokens.
 *
 * @type {import('tailwindcss').Config}
 */
module.exports = {
  content: [
    './PhilisaAbantuBethu/Pages/**/*.cshtml',
    './PhilisaAbantuBethu/wwwroot/js/**/*.js'
  ],
  theme: {
    extend: {
      colors: tokens.colors,
      fontFamily: tokens.fontFamily,
      minHeight: tokens.minHeight,
      minWidth: tokens.minWidth,
      borderRadius: { token: tokens.borderRadius }
    }
  },
  plugins: []
};

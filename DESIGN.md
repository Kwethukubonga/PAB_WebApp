# DESIGN.md – PAB Help Centre and Philisa Volunteers

How both apps look and behave: the colours, type and spacing they share, and every web component with its states. The tokens are written once in `design/tokens.json` and turned into the website's Tailwind theme and Android's `colors.xml` by `tools/tokens/build.js`, so a change there changes both apps.

---

## 1. Principles

| Principle | What it means on screen |
|-----------|-------------------------|
| **Calm, not alarming** | Lavender and cream, not red. Red is kept for urgent things only: errors, the urgent pill, the crisis line. |
| **Safe to be seen using** | No imagery that shouts "abuse", nothing that would look alarming over someone's shoulder. |
| **Plain and warm** | Short sentences, the same words in all three languages, never jokey. |
| **Works on a cheaper phone** | Self-hosted fonts (152 KB for both), no CDN, one small stylesheet, 44 px touch targets. |
| **Readable by everyone** | Every text colour pair passes WCAG AA (4.5:1). Focus is always visible. Motion stops when the phone asks for less. |

---

## 2. Colour

All colours come from `design/tokens.json`. In the pages they are Tailwind classes: `brand-*`, `ink-*` (the neutral scale), `cream`, `status-*` and `on-brand-*`. Contrast ratios were measured against the background each colour is used on.

### 2.1 Brand (PAB's lavender)

| Token | Hex | Used for | Contrast |
|-------|-----|----------|----------|
| `brand-900` | `#3B0764` | Footer band, page heroes | 15.0:1 with white text |
| `brand-800` | `#4C1D95` | Primary button hover, text on white buttons | 11.0:1 on white |
| `brand-700` | `#5B21B6` | Primary buttons, links, current language, current step | 9.0:1 on white |
| `brand-500` | `#7C3AED` | Focus outline, disabled primary button | 5.7:1 on white |
| `brand-100` | `#EDE9FE` | Focus ring on fields, current-step ring, footer text | 12.6:1 on `brand-900` |
| `brand-50` | `#F5F3FF` | Chosen radio card, hover on white buttons | `brand-700` on it: 8.2:1 |

### 2.2 Neutrals (`ink-*`)

Greys tinted slightly warm rather than flat grey. `ink-500` is the lightest colour allowed for text.

| Token | Hex | Used for | Contrast |
|-------|-----|----------|----------|
| `ink-900` | `#1F2937` | Headings and body text | 14.2:1 on cream |
| `ink-700` | `#374151` | Secondary text, field labels | 10.3:1 on white |
| `ink-600` | `#57534E` | Muted text, inactive tabs | 7.4:1 on cream |
| `ink-500` | `#6B7280` | Placeholders, step labels (the lightest text) | 4.8:1 on white, 4.7:1 on cream |
| `ink-300` | `#D6D3D1` | Field and secondary button borders | Borders only |
| `ink-200` | `#E5E7EB` | Card borders, step connector lines | Borders only |
| `ink-100` | `#F3F4F6` | Quiet backgrounds | Background only |
| `ink-50` | `#FAFAF9` | Secondary button hover | Background only |

### 2.3 Surfaces

| Token | Hex | Used for |
|-------|-----|----------|
| `cream` | `#FDFBF5` | Page background |
| `white` | `#FFFFFF` | Cards, fields, forms |

### 2.4 Status

One colour per status, used the same way on the website, the staff area and the Android app. Each text colour is paired with its own surface.

| Status | Text | Surface | Contrast | Where |
|--------|------|---------|----------|-------|
| Urgent / error | `#B91C1C` | `#FEE2E2` | 5.3:1 | Urgent pill, field errors, emergency numbers |
| New | `#5B21B6` | `#EDE9FE` | 7.6:1 | New request pill |
| In progress | `#0E7490` | `#CFFAFE` | 4.8:1 | In-progress pill |
| Closed | `#4B5563` | `#F3F4F6` | 6.9:1 | Closed pill |
| Success | `#047857` | `#D1FAE5` | 4.8:1 | Sent and saved messages |

### 2.5 Text on brand colours

| Token | Value | Used for |
|-------|-------|----------|
| `on-brand-default` | `#FFFFFF` | Headings and buttons on purple |
| `on-brand-muted` | white at 85% | Supporting text on purple (passes 4.5:1 on `brand-500` and darker) |

The website has no dark mode. The Android app has its own dark palette (`values-night/colors.xml`), chosen for dark surfaces rather than these colours inverted.

---

## 3. Type

| Role | Font | Why |
|------|------|-----|
| Headings | **Lora** (`font-serif`), bold | Warm and personal, gives the site its character |
| Body and UI | **Source Sans 3** (`font-sans`), the default | Clear at small sizes, and carries isiXhosa and Afrikaans diacritics (ê, ô, ū) properly |

Both are variable fonts, self-hosted as `woff2` in `wwwroot/fonts/` with `font-display: swap`, so text shows immediately in a fallback font (Georgia, then the system sans) and nothing about the visitor is sent to Google.

### 3.1 Scale

| Token | Size | Typical use |
|-------|------|-------------|
| `5xl` | 3rem (48px) | Page title on desktop (`text-4xl md:text-5xl`) |
| `4xl` | 2.25rem (36px) | Page title on phones |
| `3xl` | 1.875rem (30px) | Large section titles |
| `2xl` | 1.5rem (24px) | Section headings (`h2`, the most common heading) |
| `xl` | 1.25rem (20px) | Card titles |
| `lg` | 1.125rem (18px) | Small headings, lead paragraphs |
| `base` | 1rem (16px) | Body text |
| `sm` | 0.875rem (14px) | Labels, buttons, field text, helper text |
| `xs` | 0.75rem (12px) | Pills, step labels |

All sizes are in `rem`, so text grows with the browser's font-size setting.

### 3.2 Rules

- Headings: Lora, bold, `ink-900` (white on purple).
- One `h1` per page.
- No small uppercase "eyebrow" label above a heading (E4). Uppercase is only used inside pills.
- Labels that carry information, such as "Step 2 of 3", stay.

---

## 4. Spacing, Corners and Size

| Group | Values |
|-------|--------|
| **Spacing** | A 4 px step: 4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 80 px (Tailwind `1`–`20`) |
| **Corners** | `sm` 8 px · `md` 12 px · `lg` 16 px · `xl` 24 px · `full` (pill shape) |
| **Touch targets** | At least **44 px** high on the web (`min-h-target`), 48 dp on Android |
| **Content width** | `max-w-7xl` (1280 px) with 16 / 24 / 32 px side padding on phone / tablet / desktop |

Buttons, pills and the language switcher are fully rounded. Fields and choice cards use `rounded-xl` (12 px); cards use `rounded-2xl` (16 px).

---

## 5. Components

Shared component classes are defined once in `Styles/app.css` (`@layer components`) and built into `wwwroot/css/app.css` with `npm run build`. Behaviour is in `wwwroot/js/site.js` and `wwwroot/js/request-support-wizard.js`.

Every interactive component has a visible focus state: a 2 px `brand-500` outline, 2 px away from the element, shown for keyboard users only (`:focus-visible`).

### 5.1 Buttons

| Variant | Class | Looks like | Used for |
|---------|-------|------------|----------|
| Primary | `btn-primary` | `brand-700` fill, white text | The one main action on a page: Request Support, Send, Next |
| Secondary | `btn-secondary` | White, `ink-300` border, `ink-900` text | Other actions beside a primary one |
| On brand | `btn-on-brand` | White fill, `brand-800` text | Actions on a purple hero or band |

All three are pill-shaped, at least 44 px high, `text-sm` semibold, with an optional icon before the label.

| State | Primary | Secondary | On brand |
|-------|---------|-----------|----------|
| Default | `brand-700` | White, grey border | White |
| Hover | Darkens to `brand-800` | `ink-50` background | `brand-50` background |
| Focus | Purple outline | Purple outline | Purple outline |
| Busy (sending) | Lighter `brand-500`, wait cursor, label changes to "Sending…" in the page's language, `aria-busy="true"` | – | – |
| Disabled | Same as busy; a second press is ignored | – | – |

The busy state stops a form being sent twice. It resets if the visitor comes back with the browser's Back button.

### 5.2 Text fields

Inputs, selects and text areas on the Request Support and Contact forms.

| State | Looks like | Notes |
|-------|------------|-------|
| Default | White, 1 px `ink-200` border, 12 px corners, `text-sm` | At least 44 px high |
| Placeholder | `ink-500` text | An example, never the only label |
| Focus | 2 px `brand-500` ring, border hidden | |
| Error | 2 px red border, red focus ring, red message underneath | `aria-invalid="true"`, and `aria-describedby` points at the message so it is read out |
| Filled after an error | Keeps what was typed | Nothing is lost when the form comes back |

Every field has a visible `label` above it (`text-sm`, medium, `ink-700`). Required fields end with `*`.

### 5.3 Field error message

| | |
|--|--|
| **Looks like** | `text-sm`, medium, red (`text-red-700`, 6.5:1 on white), 6 px under the field |
| **Content** | What to do, not what went wrong: "Enter a South African cellphone number, like 082 123 4567" |

### 5.4 Form error banner

Shown above a form when something stops the whole form, such as an expired page.

| | |
|--|--|
| **Looks like** | Light red panel (`red-50`), `red-200` border, 12 px corners, semibold `red-800` text (7.6:1) |
| **Accessibility** | `role="alert"`, so it is read out as soon as it appears |

### 5.5 Choice cards (radio buttons)

Used for "How should we contact you?", support type and urgency.

| State | Looks like |
|-------|------------|
| Default | White card, `ink-200` border, `ink-700` text, 12 px corners |
| Hover | Border darkens to `ink-300` |
| Chosen | `brand-700` border and `brand-50` background across the whole card, not only the small circle |
| Focus | 2 px purple ring round the whole card |

### 5.6 Step indicator (Request Support)

The form is split into three steps: About you, Support needed, Confirm and submit.

| Step state | Circle | Label |
|------------|--------|-------|
| Upcoming | `ink-200` with the step number in `ink-600` | `ink-500` |
| Current | `brand-700` with a white number and a 4 px `brand-100` ring; `aria-current="step"` | `brand-700` |
| Completed | Green with a white tick; screen readers hear "1 completed" | `ink-500` |

The lines between circles turn green once the step before is complete. On phones the step names are hidden and only the circles show. **Back** is hidden on step 1 rather than greyed out. On the last step, **Next** is replaced by the Submit button, and the summary is filled in from the earlier steps.

### 5.7 Status pills (staff area)

| Pill | Class | Colours |
|------|-------|---------|
| Urgent | `pill-urgent` | Red on light red |
| New | `pill-new` | Purple on lavender |
| In progress | `pill-working` | Teal on light cyan |
| Closed | `pill-closed` | Grey on light grey |

`text-xs`, bold, uppercase, fully rounded. The pill for a status is chosen in one place, `Staff/StaffText.cs` (`StatusPill`), so the same status is always the same colour.

### 5.8 Filter tabs (staff request list)

| State | Looks like |
|-------|------------|
| Default | White pill, `ink-300` border, `ink-700` text, with a count in an `ink-100` bubble |
| Hover | Border turns `brand-700` |
| Selected | `brand-700` fill, white text, count bubble in translucent white; `aria-current="page"` |

### 5.9 Language switcher

A pill-shaped group of three links (English, isiXhosa, Afrikaans) in the header and the mobile menu.

| State | Looks like |
|-------|------------|
| Default | `ink-600` text |
| Hover | `brand-700` text |
| Current | `brand-700` fill, white text; `aria-current="true"` |

Each link carries its own `lang` attribute, so screen readers say "isiXhosa" correctly.

### 5.10 Cards

| | |
|--|--|
| **Class** | `card`: white, `ink-200` border, 16 px corners, small shadow |
| **Used for** | Resource cards, the reference number, staff request details |
| **Linked cards** | Hover deepens the shadow (`hover:shadow-md`); focus shows the purple outline on the whole card |

### 5.11 Success panel

Shown after the Contact form is sent, and on the Confirmation page.

| | |
|--|--|
| **Looks like** | Light green panel with a round `status-success` tick, a heading and a short message |
| **Confirmation page** | Also shows the reference number in a card with a **Copy** button |

### 5.12 Copy button (reference number)

| State | Looks like |
|-------|------------|
| Default | "Copy" with a copy icon |
| Copied | Label changes to "Copied" in the page's language for a moment, then changes back |

### 5.13 FAQ accordion (Resources)

| State | Looks like |
|-------|------------|
| Closed | Question with a down-chevron; `aria-expanded="false"` |
| Open | Answer shown, chevron turned 180°; `aria-expanded="true"` |

Only one answer is open at a time.

### 5.14 Mobile menu

| State | Looks like |
|-------|------------|
| Closed | Hamburger button; `aria-expanded="false"` |
| Open | Full-width menu under the header with the links and the language switcher; `aria-expanded="true"` |

### 5.15 Skip link

Off screen until the first Tab press, then shown top-left as a white pill: "Skip to content". It jumps past the header to `<main>`.

### 5.16 Error pages

| Page | When | What it shows |
|------|------|---------------|
| 404 | A page doesn't exist | "We can't find that page", links home, PAB's contact details |
| 429 | More than 5 forms from one connection in 10 minutes | "Please wait a few minutes", and the crisis line, so nobody in danger is turned away with nothing |
| 500 | Something broke | "Something went wrong", "Please try again, or call us" |

Every error page keeps the header and the emergency numbers.

### 5.17 Loading

The site is server-rendered, so there are no spinners or skeleton screens. The only waiting state is a form being sent (5.1, busy). Photos are WebP at a width the browser picks for the screen. Those below the fold load lazily, and each has its real width and height set, so the page doesn't jump while they load (`Pages/Shared/_Image.cshtml`).

---

## 6. Motion

- Short colour transitions on hover and focus only.
- Nothing moves on its own except two small cues on the home page hero: a pulsing dot and a bouncing arrow.
- When the phone or computer asks for reduced motion, every animation and smooth scroll is switched off (`prefers-reduced-motion`), including the form's own scrolling.

---

## 7. Known Differences Still to Tidy

Places where the pages don't yet use the tokens or the shared classes. None of them fails contrast, but each one means a token change won't reach that spot.

| Where | What | Should be |
|-------|------|-----------|
| `Pages/Contact.cshtml`, `Pages/RequestSupport.cshtml` | Field, label and error styles are written out as strings at the top of each page, using Tailwind's `red-600` / `red-700` | The `field`, `field-error`, `label` and `field-message` classes, using `status-urgent` |
| Form error banners | `red-50`, `red-200`, `red-800` | `status-urgentSurface` and `status-urgent` |
| Contact success panel | `green-50`, `green-200` | `status-successSurface` |
| Step indicator, completed step | `green-600` | `status-success` |
| `Pages/Confirmation.cshtml` | Tick icon drawn in `#22c55e` | `currentColor` with `text-status-success` |
| `Pages/Shared/_Layout.cshtml.css` | Leftover styles from the ASP.NET template (`#1b6ec2`) | Remove if unused |

---

## 8. Android

*To be added by the Android team: the Material theme, how the tokens map to colour roles, the dark palette, the type scale in `sp`, and the app's components (buttons, cards, chips, bottom navigation, dialogs) with their states.*

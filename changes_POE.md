# PAB Help Centre (web): project guide

> This file and `handover/` live **only on the `changes_final_POE` branch**. They never go into `develop` or `main`, and the branch is deleted before the final submission. Claude Code does not load this file by itself, so start a session with: *"Read changes_POE.md on the changes_final_POE branch (`git show origin/changes_final_POE:changes_POE.md`)."*

The public website for Philisa Abafazi Bethu (PAB), a gender-based violence and community support NPO in Lavender Hill, Cape Town. People who may be in danger use it to request support without an account; PAB staff use `/Staff/Requests` to act on those requests. Team KANTU, varsity group project (Task 3 follows Task 2, which got 176/270).

Sister repo: the Android volunteer app, `Kwethukubonga/PAB_Volunteers`. Clone both side by side (`PAB_WebApp` and `PAB_Volunteers` in the same folder): the design tokens tool assumes it.

**Team:** this website is owned by Unathi, Nuha and Ash. The Android app is owned by Tlhogi and Kwethu, who built most of what's on `develop` here (Kwethu: API, services, design tokens, CSS, images, Docker; Tlhogi: forms, rate limiting, security headers, staff area, CI deploy) and can explain it. Shared items (B3, W10, J2, J5, E2) are split per `handover/HANDOVER.md` section 4.

## Where to start

1. `handover/HANDOVER.md` (on this branch): what's done, what's left, in order, with owners. The web team's jobs are section 2.
2. `handover/IMPROVEMENT_PLAN.md` (on this branch): the full checklist (items A1–K26, W1–W12; each says how to do it and how to know it's finished; section 8 is the Android to-do in order).
3. `README.md` section 2 maps every requirement (PAB-FR01–15) to its page, code and test. Keep it true when you change behaviour.

Read the notes without leaving your work branch: `git show origin/changes_final_POE:handover/HANDOVER.md`. The same `handover/` folder is on the Android repo's `changes_final_POE` branch; keep the two copies identical.

## Stack

- ASP.NET Core Razor Pages + minimal API endpoints, .NET 10, EF Core 10 (SQLite locally; Postgres on Neon in production, with its own migrations in `Migrations/Postgres`; `Data/DatabaseProvider.cs` picks one from the connection string).
- Tailwind compiled at build time from `design/tokens.json` (`npm run build`); fonts and images are self-hosted. No CDNs: the CSP forbids them.
- Staff sign in with their PAB Firebase account (project `pab-volunteers`); the server checks the ID token and that `users/{uid}.role == "admin"` in Firestore.
- Hosting: Docker on Render's free plan. CI (`.github/workflows/dotnet-tests.yml`) tests (with a Postgres for the Postgres tests), checks the built CSS matches the tokens, reports coverage, builds the image, and deploys `main` to Render through the `RENDER_DEPLOY_HOOK` secret.

## Commands

```bash
dotnet test PhilisaAbantuBethu.Tests
npm ci && npm run build          # after changing design/tokens.json or Styles/app.css; commit the output
dotnet run --project PhilisaAbantuBethu   # http://localhost:5277
node tools/tokens/build.js --android ../PAB_Volunteers   # writes Android's colors.xml from the same tokens
```

## Layout

- `Pages/` Razor pages; `Pages/Staff/` the staff area (needs the `Staff` policy).
- `Api/` endpoint groups (`/api/support-requests`, `/api/contact`, `/api/programmes`, `/api/resources`), one error shape (problem details).
- `Services/` all reads and writes go through here; pages and the API share them. No page model touches `AppDbContext`.
- `Forms/` validators (server-side rules for both forms). `Services/PhoneNumber.cs` is the SA phone standard shared with the Android app.
- `Security/` headers + CSP, rate limiting, request logging. `Staff/` Firebase token check and staff directory.
- `Localization/` English, isiXhosa, Afrikaans via `s.Pick(en, xh, af)`. Every user-facing string needs all three.

## Rules for this project

- **Keep the notes out of the app's history:** branch your work from `develop`, never from `changes_final_POE`, and never merge `changes_final_POE` into anything. Ticking items or adding your name is a commit on `changes_final_POE` only.
- **Final submission:** delete the `changes_final_POE` branch in both repos before the final submission (`handover/HANDOVER.md` section 5). Whenever someone mentions the final submission, the deadline, merging to `main` for submission, or a release, remind them of that checklist before doing anything else.
- **No AI attribution anywhere**: no `Co-Authored-By: Claude`/Copilot trailers, no "Generated with" lines, in commits, PRs, code or comments. The README's AI Usage Declaration section is the one place AI use is described.
- **Merge PRs with "Create a merge commit"**, never squash: squashing rewrote a teammate's commits under one name once already, and the marker grades individual contribution.
- Work on `feature/<name>/<topic>` branches, PR into `develop`; `develop` → `main` is a deliberate release (it deploys).
- Never commit secrets: Firebase service account keys, `.pfx` certificates, the Render deploy hook, the data-protection certificate and its password. The Firebase *web* API key goes in Render's `Firebase__ApiKey` variable, not in git.
- Support requests describe abuse. Never log or email personal fields, keep staff pages `no-store`/`noindex`, and keep the quick exit button on every public page.
- Never delete data in the live Firebase project or the Neon database.
- Write plain, warm UK/SA English in the UI. Commit messages: short imperative summary, body says why.

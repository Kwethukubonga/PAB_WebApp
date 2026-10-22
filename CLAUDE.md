# PAB Help Centre (web)

The public website for Philisa Abafazi Bethu (PAB), a gender-based violence and community support NPO in Lavender Hill, Cape Town. People who may be in danger use it to request support without an account; PAB staff use `/Staff/Requests` to act on those requests. Team KANTU, varsity group project (Task 3 follows Task 2, which got 176/270).

Sister repo: the Android volunteer app, `Kwethukubonga/PAB_Volunteers`. Clone both side by side (`PAB_WebApp` and `PAB_Volunteers` in the same folder): the design tokens tool and the plan assume it.

## Where to start

1. Read `../PAB_Volunteers/handover/HANDOVER.md`: what's done, what's left, in order, with owners.
2. The full checklist is `../PAB_Volunteers/handover/IMPROVEMENT_PLAN.md` (items A1–K18, W1–W12; each says how to do it and how to know it's finished). Tick items there when they're done.
3. `README.md` section 2 maps every requirement (PAB-FR01–15) to its page, code and test. Keep it true when you change behaviour.

## Stack

- ASP.NET Core Razor Pages + minimal API endpoints, .NET 10, EF Core 10 (SQLite locally and on Railway today; Npgsql when the connection string points at Postgres, see `Data/DatabaseProvider.cs`).
- Tailwind compiled at build time from `design/tokens.json` (`npm run build`); fonts and images are self-hosted. No CDNs: the CSP forbids them.
- Staff sign in with their PAB Firebase account (project `pab-volunteers`); the server checks the ID token and that `users/{uid}.role == "admin"` in Firestore.
- Hosting: Docker on Railway. CI (`.github/workflows/dotnet-tests.yml`) tests, checks the built CSS matches the tokens, reports coverage, builds the image, and deploys `main` to Railway.

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

- **No AI attribution anywhere**: no `Co-Authored-By: Claude`/Copilot trailers, no "Generated with" lines, in commits, PRs, code or comments. The README's AI Usage Declaration section is the one place AI use is described.
- **Merge PRs with "Create a merge commit"**, never squash: squashing rewrote a teammate's commits under one name once already, and the marker grades individual contribution.
- Work on `feature/<name>/<topic>` branches, PR into `develop`; `develop` → `main` is a deliberate release (it deploys).
- Never commit secrets: Firebase service account keys, `.pfx` certificates, Railway tokens. The Firebase *web* API key goes in Railway's `Firebase__ApiKey` variable, not in git.
- Support requests describe abuse. Never log or email personal fields, keep staff pages `no-store`/`noindex`, and keep the quick exit button on every public page.
- Never delete data in the live Firebase project or the Railway database.
- Write plain, warm UK/SA English in the UI. Commit messages: short imperative summary, body says why.

# Threat Model – PAB Help Centre (web)

This site holds what people write about gender-based violence. If the wrong person reads it, someone could be hurt. This document lists who could misuse that information, what stops each of them, and where in the code that protection lives. It ends with the risks that are still open.

It covers the web app in this repository. The Android app's side (Firebase, Firestore rules, notifications) is in the Philisa Volunteers repository. How the parts fit together is drawn in [ARCHITECTURE.md](ARCHITECTURE.md).

---

## 1. What We Are Protecting

| Asset | Example | How sensitive | Where it lives |
|-------|---------|---------------|----------------|
| What someone wrote about their situation | `Situation`, `Extra` on a support request | Highest. Could put the person in danger. | `SupportRequests` table in Postgres (Neon), **encrypted** |
| Who asked for help and how to reach them | Name, phone, email, area, contact method | High. Shows that this person asked a GBV centre for help. | `SupportRequests` table |
| What staff did about it | Status, `StaffNotes`, `HandledBy` | High | `SupportRequests` table |
| Contact form messages | Media, partnership and general questions | Medium | `ContactInquiries` table, read on `/Staff/Messages` |
| The fact that someone visited the site | Browser history, cached pages, the address of a page | High for someone being watched | The visitor's own device |
| The keys that unlock the encrypted text | ASP.NET Data Protection keys | Highest. Anyone with them and the database can read everything. | `DataProtectionKeys` table, itself locked with a certificate held only in Render's secret settings |
| Database backups | `.dump` files made by `tools/database/Backup-Database.ps1` | High. Hold names and phone numbers. | `pab-backups` in the home folder of whoever ran the backup |
| Staff access | Session cookie `pab_staff`, Firebase ID tokens | High. Opens every request. | The staff member's browser or phone |

---

## 2. Who Could Misuse It

| # | Who | What they want |
|---|-----|----------------|
| T1 | **An abuser** | To find out if their partner asked for help, what they said, and where they are |
| T2 | **A spammer or bot** | To flood the forms, bury real requests, or waste staff time |
| T3 | **A nosy or careless insider** | A staff member, or a volunteer with a Philisa Volunteers account, reading requests they have no reason to see |
| T4 | **An outside attacker** | To break in and copy the data, take the site down, or use it to attack visitors |
| T5 | **Accidental leaks** | Not a person: logs, emails, caches and backups that end up holding personal details nobody meant to keep there |

---

## 3. Threats and What Stops Them

### T1 – An abuser

**1a. Watching the person's phone or computer.** The abuser walks in while the site is open, checks the browser later, or opens a saved page.

| What stops it | Where in the code |
|---------------|-------------------|
| **Leave this site** button, or Esc twice, swaps the page for a weather search. `location.replace()` means Back doesn't return here. | `Pages/Shared/_Layout.cshtml`, `wwwroot/js/site.js` (quick exit) |
| `Referrer-Policy: no-referrer`, so the next site visited never learns the person came from a help page | `Security/SecurityHeaders.cs` |
| The offline copy keeps only the public Resources page. Nothing typed, no reference number, no staff page, no API answer. The Resources FAQ has a button to clear it. | `wwwroot/sw.js`, `wwwroot/js/site.js` |
| No public account, so there is no password for an abuser to force out of someone, and nothing to log in to later | By design: `POST /RequestSupport` and `POST /api/support-requests` are anonymous |

**1b. Trying to find the person's request on the site.**

| What stops it | Where in the code |
|---------------|-------------------|
| There is no public page or API that shows a request, even with its reference number. Reading requests is staff only. | `Api/SupportRequestEndpoints.cs` (`RequireAuthorization(StaffApiAuth.Policy)` on every read and change) |
| Reference numbers are random (`PAB-7KQ39M`, about 887 million options), so they can't be guessed or counted through | `Services/ReferenceGenerator.cs` |
| Staff pages are never cached and never indexed by search engines | `Staff/StaffAuth.cs` (`StaffPageModel`: `Cache-Control: no-store`, `X-Robots-Tag: noindex`) |

### T2 – A spammer or bot

| What stops it | Where in the code |
|---------------|-------------------|
| Rate limit: 5 form sends per address per 10 minutes, on both the pages and the API. The 429 page still shows the crisis line. | `Security/FormRateLimiting.cs`, `Pages/StatusCode.cshtml` |
| The visitor's address can't be faked to dodge the limit: `X-Forwarded-For` is only trusted behind Render's proxy, and only the entry Render added is read (`TrustProxyHops=3`). An address a visitor makes up lands further left and is ignored. | `Program.cs` (`TrustProxy`, `TrustProxyHops`, `ForwardLimit`) |
| Hidden spam-trap field that only bots fill in. When it's filled, nothing is saved. | `Pages/RequestSupport.cshtml.cs`, `Pages/Contact.cshtml.cs` (`Website`) |
| Every field is checked again on the server: required, lengths, SA phone format, and only the offered choices | `Forms/SupportRequestForm.cs`, `Forms/ContactForm.cs`, `Services/PhoneNumber.cs` |
| Request bodies are capped at 256 KB for the site and 64 KB for the two forms, so nobody can force huge uploads | `Program.cs` (Kestrel limit), `[RequestSizeLimit]` on both form pages |

### T3 – A nosy or careless insider

| What stops it | Where in the code |
|---------------|-------------------|
| `/Staff` needs a Firebase account whose Firestore record says `role: admin`. A volunteer's valid account is refused (403). | `Program.cs` (`AuthorizeFolder("/Staff")`), `Staff/StaffDirectory.cs` |
| The admin role can only be given in the Firebase console, never from the app or the site | Firestore security rules in the Android repository |
| The API checks a bearer token is signed by Google for this project **and** that its owner is an admin. Being signed in is not enough. | `Staff/FirebaseTokenVerifier.cs`, `Staff/StaffApiAuth.cs` (`StaffRoleHandler`) |
| The request list leaves out the situation text. It is only shown one request at a time. | `Api/SupportRequestEndpoints.cs` (`SupportRequestSummary` vs `SupportRequestDetail`) |
| Status changes record who made them and when | `HandledBy`, `StatusUpdatedAt` on `Models/SupportRequest.cs` |
| Staff sessions end after 8 hours and don't slide, so a forgotten sign-in on an office computer expires | `Program.cs` (cookie `ExpireTimeSpan`, `SlidingExpiration = false`) |

### T4 – An outside attacker

| Attack | What stops it | Where in the code |
|--------|---------------|-------------------|
| Stealing a copy of the database or a backup | The situation text is encrypted at rest. The keys stored in the database are themselves encrypted with a certificate that is never in the database, so a copy of the database alone can't be read (this is tested). | `Data/Protection/SensitiveText.cs`, `Data/Protection/DataProtectionSetup.cs` |
| Faking a staff sign-in | Firebase tokens are checked against Google's keys, issuer, audience and expiry | `Staff/FirebaseTokenVerifier.cs` |
| Stealing the staff cookie with a script | Cookie is `HttpOnly` and `SameSite=Strict`; a strict Content Security Policy allows only our own scripts plus Firebase's sign-in | `Program.cs`, `Security/SecurityHeaders.cs` |
| Cross-site scripting | Razor encodes everything people type before showing it; CSP has no `unsafe-inline` for scripts | Razor Pages, `Security/SecurityHeaders.cs` |
| Cross-site request forgery | Anti-forgery token on every Razor form; `SameSite=Strict` cookie | Razor Pages default |
| SQL injection | Entity Framework only sends parameterised queries | `Data/AppDbContext.cs`, `Services/` |
| Putting the site in a hidden frame (clickjacking) | `X-Frame-Options: DENY`, `frame-ancestors 'none'` | `Security/SecurityHeaders.cs` |
| Reading traffic on the network | HTTPS everywhere, HSTS in production | `Program.cs` (`UseHttpsRedirection`, `UseHsts`) |
| Sending someone to an outside site after staff sign-in | The return address must be on this site | `Pages/Staff/SignIn.cshtml.cs` (`Url.IsLocalUrl`) |
| A bug letting code run on the server | The container runs as a non-root user | `Dockerfile` (`USER app`) |
| A deploy with missing secrets quietly storing keys unprotected | The site refuses to start without the certificate when keys are in the database, so Render keeps serving the old version | `Data/Protection/DataProtectionSetup.cs` |
| A known hole in a library | Dependabot opens weekly update PRs; CI builds and tests every change | `.github/dependabot.yml`, `.github/workflows/dotnet-tests.yml` |
| Secrets in the code | No secrets in Git. The Firebase API key, database connection string, key certificate and Resend API key are Render environment settings; the Render deploy hook is a GitHub secret. `*.db` files and backups are excluded from Git. | `appsettings.json` (empty values), `.gitignore`, `.dockerignore` |

### T5 – Accidental leaks

| What stops it | Where in the code |
|---------------|-------------------|
| Logs hold only method, path, status and time. No addresses, no query strings, nothing from a form. IDs and references in a path become `{id}`. | `Security/RequestLogging.cs` |
| A decryption failure is logged without the row's contents | `Data/Protection/SensitiveText.cs` |
| Support request emails carry only the reference, support type (in English), urgent yes/no, time received and a link into the staff area. Contact emails carry only the subject and time. Never a name, phone, email, area, situation or message text, because inboxes get forwarded and synced to phones. | `Services/StaffAlerts.cs` (`ForSupportRequest`, `ForContactMessage`) |
| A failed email never stops a request saving: sending has an 8-second limit and any failure is only a warning, logged with the reference, never a name | `Services/StaffAlerts.cs` (`SendAsync`) |
| Emails go through Resend's HTTPS API, and with no API key set nothing is sent, so a laptop or test run can't email PAB by accident | `Services/EmailSender.cs` |
| Closed requests and contact messages are deleted after 12 months (POPIA). Open requests are never deleted. | `Services/RetentionService.cs` |
| API errors come back in one standard shape that never includes stack traces or data | `Api/ApiErrorMiddleware.cs`, `Api/ApiProblem.cs` |

---

## 4. Summary by STRIDE

| STRIDE category | Main threat here | Main control |
|-----------------|------------------|--------------|
| **S**poofing | Pretending to be PAB staff | Firebase token check + Firestore admin role |
| **T**ampering | Changing a request or its status | Staff-only API and pages; anti-forgery tokens; server-side validation |
| **R**epudiation | Denying who closed a request | `HandledBy` and `StatusUpdatedAt` recorded on every status change |
| **I**nformation disclosure | Someone reading what a person wrote | Encryption at rest, staff-only reads, no personal data in logs or emails, quick exit, no-referrer |
| **D**enial of service | Flooding the forms | Rate limiting, request size limits, spam trap |
| **E**levation of privilege | A volunteer acting as an admin | Role only set in the Firebase console; checked on every API call |

---

## 5. Risks Still Open

These are known gaps. Each one has a suggested next step.

| # | Risk | Why it matters | Suggested next step |
|---|------|----------------|---------------------|
| R1 | Name, phone, email and area are not encrypted. Only the situation text is. | A stolen database still shows who asked for help. | Encrypt these columns too, or at least phone and email. Staff search would need to change. |
| R2 | The database connection to Neon is encrypted but set to `Trust Server Certificate=true`. | The site doesn't prove it is talking to Neon itself, so a well-placed attacker could pretend to be the database. | Turn certificate checking on in `Data/DatabaseProvider.cs`. |
| R3 | `StaffNotes` are not encrypted. | Staff may write sensitive details there. | Encrypt `StaffNotes` the same way as `Situation`. |
| R4 | Viewing a request is not logged, only status changes. | A nosy staff member could read requests without leaving a trace. | Log "staff X opened request Y" (no contents) for the detail page and API. |
| R5 | Every admin can see every request. | Volunteers with the admin role may not need GBV details. | A separate "case worker" role for the staff area. |
| R6 | Staff sign-in strength depends on Firebase. Two-factor sign-in isn't required. | A guessed or phished admin password opens every request. | Turn on multi-factor authentication for admin accounts in Firebase. |
| R7 | Pages a person visited stay in their browser history; the quick exit only replaces the current page. | An abuser checking the history can see the site was used. | Add a short "browse safely" tip (private window, clear history) on the Home and Request Support pages. |
| R8 | The rate limit is per address. | Many machines can still send spam, and people sharing one address (a clinic's Wi-Fi) share a limit. | Watch the logs once live; add a CAPTCHA only if spam gets through. |
| R9 | If the key certificate is lost, saved situation text can never be opened again. | Old requests become unreadable to staff. | Keep the certificate in Render's settings and a password manager only, and run `tools/database/Test-Restore.ps1` after any change. |
| R10 | The API error handler logs the raw path, so a failed `/api/support-requests/41` is logged with its id rather than `{id}`. | Low: an id isn't personal, but it is inconsistent with the logging rule. | Use `RequestLoggingMiddleware.Shape()` in `Api/ApiErrorMiddleware.cs` too. |
| R11 | Backups are made by hand and saved on a team member's computer. | They contain names and phone numbers, and a laptop can be lost or shared. | Schedule backups to private storage, encrypt the backup folder, and delete old ones. |

---

*Last reviewed against the `develop` branch on 25 October 2026 (Render hosting, Neon Postgres, Resend email alerts).*

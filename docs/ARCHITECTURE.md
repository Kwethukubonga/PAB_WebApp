# Architecture – PAB Help Centre (web)

The Help Centre is one ASP.NET Core app (.NET 10, Razor Pages + minimal API) running in a Docker container on Render. It saves support requests and contact messages to a Postgres database on Neon, emails PAB through Resend when something new comes in, and uses Firebase only to check who PAB staff are, so staff sign in with the same admin account they use in the Philisa Volunteers app.

This page covers the web side. The Android side (Firebase, Firestore rules, notifications) is documented in the Philisa Volunteers repository. Who could misuse the data and what stops them is in [THREAT_MODEL.md](THREAT_MODEL.md).

---

## 1. System Diagram

The dashed boxes are trust boundaries: data crossing one of them is checked before it is trusted.

```mermaid
flowchart LR
    subgraph Public["Public internet - untrusted"]
        Visitor["Visitor's browser<br/>forms, quick exit, offline Resources"]
    end

    subgraph StaffSide["PAB staff - signed in"]
        StaffBrowser["Staff browser<br/>/Staff pages"]
        AndroidAdmin["Philisa Volunteers app<br/>admin screen"]
    end

    subgraph Render["Render"]
        Proxy["Render proxy<br/>HTTPS ends here"]
        Secrets["Environment settings<br/>key certificate, API keys,<br/>connection string"]
        subgraph App["ASP.NET Core container - non-root user"]
            Pipeline["Middleware<br/>security headers, HTTPS,<br/>logging, rate limit, auth"]
            Pages["Razor Pages<br/>Pages/, Pages/Staff/"]
            Api["Minimal API<br/>Api/"]
            Forms["Validation<br/>Forms/"]
            Services["Services/<br/>requests, contact, alerts,<br/>references, retention"]
            Protect["Encryption<br/>Data/Protection/"]
            Db["EF Core<br/>Data/"]
        end
    end

    subgraph Neon["Neon - Postgres"]
        Tables[("SupportRequests<br/>ContactInquiries")]
        Keys[("DataProtectionKeys<br/>locked with the certificate")]
    end

    subgraph Google["Firebase - Google"]
        FbAuth["Firebase Authentication<br/>signs ID tokens"]
        Firestore[("Firestore<br/>users/uid.role")]
    end

    Resend["Resend<br/>alert emails, no personal data"]
    Inbox["PAB inbox"]
    GitHub["GitHub Actions<br/>test, build, deploy main"]

    Visitor -- "HTTPS" --> Proxy
    StaffBrowser -- "HTTPS + pab_staff cookie" --> Proxy
    AndroidAdmin -- "HTTPS + Bearer ID token" --> Proxy
    StaffBrowser -- "sign in" --> FbAuth
    AndroidAdmin -- "sign in" --> FbAuth

    Proxy --> Pipeline
    Pipeline --> Pages
    Pipeline --> Api
    Pages --> Forms
    Api --> Forms
    Forms --> Services
    Services --> Db
    Db --> Protect
    Secrets -. "certificate" .-> Protect
    Protect -. "unlocks keys" .-> Keys
    Db -- "encrypted connection" --> Tables

    Pipeline -- "check token signature" --> FbAuth
    Pipeline -- "is this person an admin?" --> Firestore
    Services -- "HTTPS API" --> Resend
    Resend --> Inbox
    GitHub -- "deploy hook, then /health" --> Proxy

    classDef boundary stroke-dasharray: 6 4
    class Public,StaffSide,Render,Neon,Google boundary
```

Locally the same app runs on a SQLite file with its keys in a folder, so nobody needs Neon or the certificate to work on it.

---

## 2. What Each Part Does

| Part | Where | Job |
|------|-------|-----|
| Middleware | `Program.cs`, `Security/` | Runs on every request, in this order: forwarded headers (behind Render only), error pages, security headers, HTTPS redirect, request logging, rate limiting, sign-in, permission checks, language cookie |
| Razor Pages | `Pages/` | The public pages and the two forms. `Pages/Staff/` is the staff area: support requests and contact messages. |
| Minimal API | `Api/` | The same features as JSON for the Android app: send a request or message, read programmes and resources, and (staff only) list, read and update requests |
| Validation | `Forms/` | Checks every field on the server. Shared by the pages and the API, so both follow the same rules. |
| Services | `Services/` | Saving and reading requests, reference numbers, the 12-month cleanup, and the alert emails (`StaffAlerts.cs`, `EmailSender.cs`) |
| Encryption | `Data/Protection/` | Encrypts `Situation` and `Extra` on the way into the database, and keeps the keys in the database locked with a certificate (`DataProtectionSetup.cs`) |
| Database access | `Data/` | Entity Framework Core. Postgres when the connection string points at one, SQLite otherwise (`Data/DatabaseProvider.cs`). Migrations run when the site starts. |
| Staff sign-in | `Staff/` | Checks Firebase ID tokens and the `admin` role in Firestore |
| Render | (host) | Builds the Docker image, ends HTTPS, holds the secret settings, and only switches to a new version once `/alive` answers |
| Neon | (host) | Managed Postgres in Frankfurt, the same region as Render |
| Firebase | (Google) | Signs staff in and holds the `role` on each user. The site keeps no list of staff of its own. |
| Resend | (email) | Sends the short "something new came in" emails to PAB's inbox over HTTPS |
| GitHub Actions | `.github/workflows/` | Runs the tests and Docker build on every PR; on `main`, triggers Render's deploy hook and checks the live `/health` |

---

## 3. Key Flows

### 3.1 Someone asks for support

```mermaid
sequenceDiagram
    actor V as Visitor
    participant P as Render proxy
    participant A as App (RequestSupport)
    participant F as Forms/
    participant S as Services/
    participant D as Postgres
    participant R as Resend

    V->>P: POST /RequestSupport (HTTPS)
    P->>A: forwards, adds client address
    A->>A: rate limit (5 per 10 min), anti-forgery, spam trap
    A->>F: validate every field
    F-->>A: errors? back to the form, nothing saved
    A->>S: save
    S->>S: random reference PAB-XXXXXX
    S->>D: insert, Situation and Extra encrypted
    S->>R: alert: reference, type, urgent, time only
    R-->>S: sent, or a warning logged (request already saved)
    S-->>A: reference
    A-->>V: Confirmation page with reference
```

### 3.2 A staff member signs in

```mermaid
sequenceDiagram
    actor St as Staff member
    participant FA as Firebase Auth
    participant A as App (/Staff/SignIn)
    participant FS as Firestore

    St->>FA: email + password, or Google
    FA-->>St: ID token
    St->>A: POST ID token
    A->>A: check Google's signature, issuer, audience, expiry
    A->>FS: read users/uid with the person's own token
    FS-->>A: role
    alt role is admin
        A-->>St: 8-hour pab_staff cookie (HttpOnly, SameSite Strict)
    else not admin
        A-->>St: 403, "This account isn't PAB staff"
    end
```

The Android admin screen skips the cookie: it sends its Firebase ID token as `Authorization: Bearer` on each API call, and the same two checks run every time.

---

## 4. Android Side

*To be added by the Android team: Firebase Authentication, Firestore and its security rules, the notifications worker, and how the admin screen calls this site's API.*

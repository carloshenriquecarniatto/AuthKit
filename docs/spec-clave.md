# AuthKit v0.2 — Cl@ve Module Specification

**English** · Português · Español _(translations pending)_

Status: **draft, normative once accepted**. Target contract version: **1.1.0** (additive). Owner:
AuthKit maintainers. Supersedes nothing; extends [`spec.md`](spec.md).

---

## 1. Goal

Let an AuthKit host authenticate Spanish citizens through **Cl@ve** — and, through the same
abstraction, through the certificate and EUDI wallet routes that follow it — without any AuthKit
component ever touching SAML.

The module ships in two halves that meet at a documented HTTP boundary:

```text
┌──────────────────────────┐        ┌─────────────────────────────┐        ┌──────────────┐
│ AuthKit.Clave            │  HTTP  │ Federation backend          │  SAML  │ Pasarela     │
│ (UI: begin / complete)   │ ─────► │ (owns eIDAS, certs, CAID)   │ ─────► │ Cl@ve        │
└──────────────────────────┘        └─────────────────────────────┘        └──────────────┘
```

AuthKit ships the left box. It does **not** ship the middle box in v0.2. The middle box is the
host's responsibility, and any implementation that satisfies §5 works — including a Keycloak
deployment, the official PAe integration kit, or a commercial broker.

## 2. Why this is not `SocialProviders`

The obvious shortcut is `BeginOAuthAsync("clave")`. It is rejected because Cl@ve differs from a
social IdP in four ways the contract must express:

| Difference                    | Consequence                                                                                       |
| ----------------------------- | ------------------------------------------------------------------------------------------------- |
| Assurance level is a request  | The caller chooses `low` / `substantial` / `high`; the result carries the level actually granted  |
| Method is a request           | Cl@ve Móvil, Permanente, PIN, certificate and DNIe are different user journeys, not "providers"   |
| The browser never sees tokens | The pasarela posts the assertion to the backend, which then redirects to the app with a code      |
| The identity is a legal one   | The subject is a DNI/NIE with a document type, not an email, and cannot be used as a database key |

Encoding the method in the provider string (`"clave:high:movil"`) would hide all of this inside a
`string`. The contract gets explicit types instead.

## 3. Packages and ownership

| Artifact                          | Target     | Responsibility                                                      |
| --------------------------------- | ---------- | ------------------------------------------------------------------- |
| `AuthKit.Clave`                   | net10.0    | `ClaveFederationProvider`, options, tenant model, error mapping, DI |
| `AuthKit.Blazor` (extend)         | net10.0    | `<AuthKitClave>`, `<AuthKitClaveCallback>`                          |
| `@authkit/angular` (extend)       | Angular 22 | `<ak-clave>`, `<ak-clave-callback>`, `provideAuthKitClave()`        |
| `contracts/authkit.contract.json` | —          | Contract **1.1.0**, additive only                                   |

Versioning stays lockstep across all packages per `spec.md` §8. No new npm scope, no new NuGet
prefix.

### 3.1 Non-goals for 0.2

Explicitly out of scope, and each is a future document:

- Parsing or validating SAML (see §4.1).
- Managing SP certificates, CAID registration or metadata exchange.
- Cl@ve Firma (document signature). Signature is a three-phase flow with Autofirma and is **not** an
  authentication concern; it belongs to `spec-signature.md`.
- Employee/staff login, LDAP, or internal directory federation.
- Any UI for administering entities or certificates.

## 4. Architecture

### 4.1 Hard boundary: no SAML in the client

`AuthKit.Clave` must never construct, parse, validate or log a SAML document. This is a
release-blocking rule (R-01), not a preference:

- XML signature validation is where the vulnerabilities live; a UI kit is the wrong place to attempt
  it and the wrong place to be trusted with the assertion.
- AuthKit runs in WebAssembly and in browsers. A client that validates signatures invites hosts to
  trust client-side validation.
- The backend already has to do it, because it owns the certificate and the ACS URL.

Everything the module receives is an opaque `code`, a `state`, and a token endpoint response.

### 4.2 Flow

```text
1. BeginFederationAsync
   UI ──► GET  {backend}/authorize?tenant=…&provider=clave&loa=substantial
                                        &method=clave_movil&redirect_uri=…&state=…&code_challenge=…
   backend stores state+verifier, builds the eIDAS AuthnRequest, signs it
   backend ──► 302 to pasarela

2. User authenticates at Cl@ve (Móvil / Permanente / certificado / DNIe)

3. Pasarela ──► POST {backend}/acs   (SAMLResponse, RelayState)
   backend validates signature, Destination, InResponseTo, time window
   backend resolves the entity, maps PersonIdentifier, issues code
   backend ──► 302 {redirect_uri}?code=…&state=…

4. CompleteFederationAsync(callbackUrl)
   UI ──► POST {backend}/token   grant_type=authorization_code&code=…&code_verifier=…&client_id=…
   backend ──► { access_token, refresh_token, expires_in, token_type, identity }
   UI stores the session via IAuthSessionStore and raises StateChanged
```

The `state` and PKCE verifier are held by the existing `IOAuthStateStore` abstraction, reused as is.
Only the newest pending attempt is kept, matching the anti-replay rule already documented in
`AuthKit.Supabase`.

### 4.3 Where the two products meet

The backend contract in §5 is the same contract consumed by the TypeScript `@clavejs/*` client. One
contract, two clients, zero duplicated protocol work. A change to §5 is a change to both and
requires a version bump here and there.

## 5. Federation backend contract v1

The module is only as good as this contract. It is deliberately OAuth2-shaped so that any competent
backend, in any language, can implement it.

### 5.1 Discovery

`GET {baseUrl}/.well-known/authkit-federation`

```json
{
  "contract_version": "1",
  "issuer": "https://auth.example.es",
  "authorization_endpoint": "https://auth.example.es/authorize",
  "token_endpoint": "https://auth.example.es/token",
  "session_endpoint": "https://auth.example.es/session",
  "logout_endpoint": "https://auth.example.es/logout",
  "jwks_uri": "https://auth.example.es/jwks",
  "grant_types": ["authorization_code", "refresh_token"],
  "code_challenge_methods": ["S256"],
  "tenants": [
    {
      "id": "caib",
      "display_name": "Govern de les Illes Balears",
      "providers": [
        {
          "id": "clave",
          "kind": "national_identity",
          "display_name": "Cl@ve",
          "methods": ["clave_movil", "clave_permanente", "clave_pin"],
          "levels": ["substantial", "high"]
        },
        {
          "id": "certificate",
          "kind": "certificate",
          "display_name": "Certificado electrónico",
          "methods": ["certificate", "dnie"],
          "levels": ["high"]
        }
      ]
    },
    {
      "id": "palma",
      "display_name": "Ajuntament de Palma",
      "providers": ["…"]
    }
  ]
}
```

`tenants` is the multi-entity model made explicit: every public entity is a separate service
provider in the CAID, with its own NIF, certificate, ACS URL and metadata. AuthKit treats tenancy as
data, never as configuration-in-code.

### 5.2 Authorization

`GET {authorization_endpoint}`

| Parameter               | Required | Notes                                                             |
| ----------------------- | -------- | ----------------------------------------------------------------- |
| `tenant`                | yes      | Tenant identifier from discovery. Never inferred server-side      |
| `provider`              | yes      | `clave`, `certificate`, later `eudi_wallet`                       |
| `loa`                   | no       | `low` \| `substantial` \| `high`; backend default applies         |
| `method`                | no       | Narrows the user journey; backend may ignore                      |
| `redirect_uri`          | yes      | Must exactly match a registered value; no wildcards, no fragments |
| `state`                 | yes      | Opaque, high entropy, single use                                  |
| `code_challenge`        | yes      | `S256`                                                            |
| `code_challenge_method` | yes      | `S256` only                                                       |
| `prompt`                | no       | `login` forces re-authentication (maps to `forceAuthn`)           |

The backend responds `302` to the IdP. It must not respond `200` with a document.

### 5.3 Token

`POST {token_endpoint}`, `application/x-www-form-urlencoded`

| Grant                | Body                                                 | Response                                                                |
| -------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------- |
| `authorization_code` | `code`, `code_verifier`, `client_id`, `redirect_uri` | `access_token`, `refresh_token`, `expires_in`, `token_type`, `identity` |
| `refresh_token`      | `refresh_token`, `client_id`                         | same, with a **rotated** refresh token                                  |

`identity` is the only AuthKit-specific member:

```json
{
  "subject": "9f1c…",
  "document": { "type": "dni", "value": "12345678Z", "raw": "ES/ES/12345678Z" },
  "given_name": "MARÍA",
  "family_name": "GARCÍA LÓPEZ",
  "date_of_birth": "1985-03-14",
  "loa": "substantial",
  "method": "clave_movil",
  "provider": "clave",
  "tenant": "caib",
  "issued_at": "2026-10-22T09:14:03Z",
  "represented": null
}
```

Rules:

1. `document.value` is normalised (uppercase, no separators, no country prefix). `document.raw`
   preserves what the pasarela sent, for audit.
2. `subject` is an **opaque per-tenant identifier**, stable across logins. It is not the DNI.
3. `represented` carries the acting-on-behalf-of document when the backend resolved a power of
   attorney; it is `null` otherwise. Representation is a backend concern in v0.2.
4. `loa` is the level **granted**, never the level requested.

### 5.4 Session, logout, JWKS

- `GET {session_endpoint}` with `Authorization: Bearer` returns the same `identity` plus expiry;
  used by `IInitializableAuthProvider` to restore.
- `POST {logout_endpoint}` revokes the refresh token and, when `post_logout_redirect_uri` is
  present, returns `302`. Cl@ve does not implement single logout; the backend clears its own session
  only.
- `GET {jwks_uri}` publishes signing keys for **resource servers**, not for AuthKit.

### 5.5 Errors

The backend returns RFC 6749 shaped errors:

```json
{ "error": "loa_not_met", "error_description": "…" }
```

`error_description` is for operators and **must never reach the UI** (T-07).

## 6. Contract changes — 1.1.0 (additive)

### 6.1 New types

```json
{
  "AuthDocumentType": {
    "enum": ["dni", "nie", "cif", "unknown"]
  },
  "AuthIdentity": {
    "fields": {
      "subject": "string",
      "documentType": "AuthDocumentType",
      "documentId": "string",
      "documentIdRaw": "string?",
      "givenName": "string?",
      "familyName": "string?",
      "dateOfBirth": "string?",
      "loa": "AuthAssuranceLevel",
      "method": "string?",
      "provider": "string",
      "tenant": "string",
      "representedDocumentId": "string?",
      "representedDocumentType": "AuthDocumentType?"
    }
  },
  "AuthAssuranceLevel": {
    "enum": ["low", "substantial", "high"]
  },
  "AuthFederationProvider": {
    "fields": {
      "id": "string",
      "kind": "AuthFederationKind",
      "displayName": "string",
      "methods": "string[]",
      "levels": "AuthAssuranceLevel[]"
    }
  },
  "AuthFederationKind": {
    "enum": ["national_identity", "certificate", "wallet", "enterprise_sso"]
  },
  "AuthFederationTenant": {
    "fields": {
      "id": "string",
      "displayName": "string",
      "providers": "AuthFederationProvider[]"
    }
  }
}
```

### 6.2 Extended types

| Type               | Change                                                                                                                        |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| `AuthUser`         | `identity: AuthIdentity?` — additive; `id` remains the stable opaque subject                                                  |
| `AuthCapabilities` | `federation: AuthFederationTenant[]` — additive; `socialProviders` unchanged                                                  |
| `AuthErrorCode`    | `federation_cancelled`, `federation_failed`, `loa_not_met`, `certificate_invalid`, `identity_not_verified`, `tenant_mismatch` |
| `AuthState`        | `redirecting` — additive, entered while the browser leaves the app                                                            |

`AuthUser.id` and `AuthIdentity.subject` are the same value. The DNI lives only in
`AuthIdentity.documentId` and must never be used as a primary key, a metric tag or a log field
(R-09).

### 6.3 New optional interface

Following the existing `IInitializableAuthProvider` pattern, so implementors of `IAuthProvider` are
not broken:

```csharp
public interface IFederatedAuthProvider : IAuthProvider
{
    /// Builds the authorization URL. The host navigates; AuthKit never navigates.
    Task<string> BeginFederationAsync(
        AuthFederationRequest request,
        CancellationToken cancellationToken = default);

    /// Completes the round trip from the URL the backend redirected to.
    Task<AuthResult> CompleteFederationAsync(
        string callbackUrl,
        CancellationToken cancellationToken = default);

    /// Ends the backend session when the provider can, without claiming a global logout.
    Task SignOutFederationAsync(
        string? postLogoutRedirectUri = null,
        CancellationToken cancellationToken = default);
}

public sealed record AuthFederationRequest
{
    public required string Tenant { get; init; }
    public required string Provider { get; init; }
    public AuthAssuranceLevel? Level { get; init; }
    public string? Method { get; init; }
    public string? RedirectTo { get; init; }
    public bool ForceReauthentication { get; init; }
}
```

`BeginOAuthAsync` must throw `AuthKitConfigurationException` for `"clave"` in `AuthKit.Clave`; a
national identity is not a social provider, and silently accepting it would let a host skip the
tenant and LoA parameters.

## 7. Provider options

```csharp
public sealed class ClaveFederationOptions
{
    public required Uri BackendBaseUrl { get; set; }        // https only; loopback allowed in Development
    public required string ClientId { get; set; }
    public string? DefaultTenant { get; set; }
    public AuthAssuranceLevel DefaultLevel { get; set; } = AuthAssuranceLevel.Substantial;
    public string? RedirectTo { get; set; }
    public TimeSpan DiscoveryCacheTtl { get; set; } = TimeSpan.FromMinutes(10);
    public bool UseDiscovery { get; set; } = true;
    public IReadOnlyDictionary<string, AuthFederationTenant>? StaticTenants { get; set; }
}
```

Startup validation, mirroring `T-04` in the threat model:

1. `BackendBaseUrl` must be `https`, except `http://localhost` and `http://127.0.0.1` in
   `Development`.
2. If discovery is enabled, the document is fetched once at startup and a failure is fatal with an
   actionable message.
3. If more than one tenant is discovered and no `DefaultTenant` is configured, startup fails rather
   than guessing.
4. `ClientId` must not look like a secret (length and entropy checks); the design is a public client
   with PKCE, and any embedded secret is rejected.

## 8. UI

### 8.1 Components

| Blazor                   | Angular               | Behaviour                                                                                                                                                               |
| ------------------------ | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `<AuthKitClave>`         | `<ak-clave>`          | Renders one button per available provider, or method buttons when the tenant declares more than one; optional LoA selector when the tenant declares more than one level |
| `<AuthKitClaveCallback>` | `<ak-clave-callback>` | Reads `code`/`state`/`error`, calls `CompleteFederationAsync`, strips the query, raises navigation                                                                      |

Both derive from `IAuthProvider` capabilities only. A host that configures no federation renders
nothing — the existing "render only what is declared" rule already covers this.

### 8.2 Branding

Cl@ve has official corporate identity rules. The component must:

- use the official logotype supplied by the entity, never a re-drawn or recoloured version;
- never present Cl@ve as an AuthKit feature ("Cl@ve" and "AuthKit" must not be visually merged);
- keep the attribution text returned by the pasarela (`ProviderName`) unchanged.

Assets are **not** bundled: hosts supply the logo URL through the theme, and the package ships a
neutral placeholder that says "Cl@ve" as text. Shipping government marks in a NuGet/npm artifact
invites both trademark and integrity problems.

### 8.3 Messages

New `AuthMessageKey` values in 1.1.0: `federationSignIn`, `federationRedirecting`,
`federationCancelled`, `federationFailed`, `loaNotMet`, `chooseMethod`, `chooseAssurance`.

English, Portuguese and Spanish ship in `AuthMessages`, matching the existing three README
languages. Spanish text must not claim to be an official Cl@ve surface.

## 9. Security requirements (release blocking)

Continuing the numbering style of `spec.md` §5.

| ID   | Requirement                                                                                                            |
| ---- | ---------------------------------------------------------------------------------------------------------------------- |
| R-01 | No SAML parsing, construction, validation or logging in any AuthKit client artifact                                    |
| R-02 | `state` is 256 bits from a CSPRNG, single use, stored server-side by the backend and client-side by `IOAuthStateStore` |
| R-03 | PKCE `S256` mandatory; `plain` is rejected at startup                                                                  |
| R-04 | `redirect_uri` is configuration, never user input; AuthKit sends the configured value verbatim                         |
| R-05 | Codes and tokens are removed from the URL and from history before any state change is raised                           |
| R-06 | No token, code or assertion in any log, exception message, telemetry tag or metric label                               |
| R-07 | `error_description` and backend error text never reach the UI; mapping is by machine code                              |
| R-08 | A granted `loa` below the requested one is a failure (`loa_not_met`), never a silent downgrade                         |
| R-09 | `subject` is opaque; the document identifier is never a primary key, a metric tag or a log field                       |
| R-10 | Tenant is explicit; a callback whose `state` belongs to another tenant is rejected (`tenant_mismatch`)                 |
| R-11 | Refresh tokens rotate; a replayed refresh token invalidates the family                                                 |
| R-12 | The component never navigates on its own; the host owns navigation and can therefore enforce origin checks             |
| R-13 | Concurrent `CompleteFederationAsync` calls for the same `state` resolve to one result                                  |
| R-14 | Cl@ve branding follows §8.2; no recolouring, no merged identity                                                        |
| R-15 | A `signature_invalid` reported by the backend surfaces as generic `federation_failed`                                  |

## 10. Threat model additions

Appended to `docs/threat-model.md` as T-10 … T-17.

| ID   | Threat                                                                   | Control                                                                                             | Test                                                                                                   |
| ---- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| T-10 | Forged SAML assertion reaches the app                                    | Out of client scope by R-01; backend validates; client cannot be tricked into parsing               | Design review + a test asserting no `System.Security.Cryptography.Xml` reference in the client package |
| T-11 | Login CSRF — attacker injects their own `code` into the victim's browser | `state` bound to the browser session and consumed once                                              | `state_from_another_session_is_rejected`                                                               |
| T-12 | Code replay after a successful login                                     | One-time code, short TTL, single-flight completion (R-13)                                           | `second_completion_of_the_same_code_fails`                                                             |
| T-13 | LoA downgrade — user lands authenticated with a weaker credential        | Requested vs granted compared, mismatch fails (R-08)                                                | `granted_loa_below_requested_fails`                                                                    |
| T-14 | Tenant confusion — a `caib` session used against a `palma` app           | Tenant asserted in `state` and in the token, compared at completion (R-10)                          | `cross_tenant_callback_is_rejected`                                                                    |
| T-15 | DNI leakage through logs, metrics or exception messages                  | R-09 plus an assertion in the existing redaction tests                                              | `document_id_is_not_logged`                                                                            |
| T-16 | Open redirect through `redirect_uri` or `post_logout_redirect_uri`       | Configuration only, exact match, no wildcards (R-04)                                                | `redirect_to_user_input_is_refused`                                                                    |
| T-17 | Session fixation across tabs in Blazor WebAssembly                       | New session replaces the stored one; storage write is last-writer-wins with a monotonic `issued_at` | `stale_session_does_not_overwrite_newer`                                                               |

## 11. Quality gates

Added to `spec.md` §7. All block the release.

1. **Contract parity.** `ContractParityTests` and `contract.spec.ts` cover every type in §6,
   including enum wire values. A missing side fails the build.
2. **Transport stubs only.** No test reaches the network; a `HttpMessageHandler` stub replays the
   fixtures in `samples/messages/clave/`.
3. **Fixture corpus**, committed and anonymised: `token-clave-movil-substantial.json`,
   `token-certificate-high.json`, `error-loa-not-met.json`, `error-federation-cancelled.json`,
   `error-tenant-mismatch.json`, `discovery-two-tenants.json`, `callback-missing-state.json`.
4. **Client purity check.** A test asserts `AuthKit.Clave` has no reference to
   `System.Security.Cryptography.Xml` and no SAML/XmlDsig package (R-01).
5. **Pentest addition.** `security/AuthKit.Pentest` gains the T-11 … T-17 attacks; Blazor and
   Angular both covered, as the existing suite already does.
6. **Docs.** `docs/clave.md` in three languages, plus `llms.txt` updated.

## 12. Delivery plan

| Milestone | Content                                                                                                                                                    | Exit criterion                                      |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| `0.2.0`   | Contract 1.1.0, `AuthKit.Clave` with discovery, begin/complete, error mapping, DI; Blazor and Angular components; fixtures; T-11…T-17 in the pentest suite | Gate green; demo app logs in against a stub backend |
| `0.2.1`   | Reference backend adapter for the demo app (not published)                                                                                                 | Demo app logs in against preproducción Cl@ve        |
| `0.2.2`   | `session_endpoint` restore path and refresh rotation tests                                                                                                 | Restore across reload verified                      |
| `0.3.0`   | `eudi_wallet` as a `wallet` kind in the same federation list                                                                                               | —                                                   |
| separate  | `spec-signature.md` — Cl@ve Firma, Autofirma, three-phase signature                                                                                        | —                                                   |

## 13. Open questions

1. **Tenant resolution on Blazor Server.** The component can take `Tenant` as a parameter, but the
   tenant must also be pinned in the circuit to avoid a parameter tampered on the client. Proposal:
   the parameter selects from the discovered list only, and the server re-asserts it on completion.
2. **Discovery versus static configuration.** Discovery is convenient and matches how the CAID model
   works, but it makes AuthKit depend on a backend being reachable at startup. The `StaticTenants`
   escape hatch exists for hosts that cannot accept that.
3. **`AuthUser.Id` migration.** Existing Supabase-backed deployments use the Supabase UUID. A host
   switching providers keeps the old `id` only if it stores the mapping; the spec should state that
   provider migration is out of scope and that identity linking is the host's job.
4. **Angular packaging.** `provideAuthKitClave()` grows `@authkit/angular`; if bundle size becomes a
   problem, the federation entry point becomes a secondary entry point (`@authkit/angular/clave`)
   rather than a new package.
5. **Brand assets.** Confirm with the entity whether the official logotype may be redistributed in a
   public sample. Default assumption: no.
